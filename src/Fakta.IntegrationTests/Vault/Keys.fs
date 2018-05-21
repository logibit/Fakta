module Fakta.IntegrationTests.Keys

open System
open Hopac
open Expecto
open Fakta
open Fakta.Logging
open Fakta.Vault
open System.Text

let pgpKey = "-----BEGIN PGP PUBLIC KEY BLOCK-----
Version: OpenPGP.js v2.0.0
mQENBFda0SUBCADaUqQYQPjPm1eMTpj0pf7o6lBO3L/xH16+/XBzAPTBaShZrpnf
4uIbGEtI9q0b5PQEPHG2xLY6RM60++IZDwXEDy61Gp0L0JQFTECw080XGxSU3+7k
3O2l9Y22EruCRolnyP6gRYU66IWpYgGsF7UON+drtwwahaz54E54sYKsRafnMHqQ
6op5/S7Ow7p1Dl6oGD3IEYJnNkEBdI5IzAOlB34/BACynPNhKbzN2cSbx9c+R+Vr
1/+cJZuCtduxETeufafDjqvD25/QQ79G2zjKpbCwc8czEaXVF9NbDowc8plfwvWv
NRp+rk0Y+lVlFwWlKfitP6RHUG3stN4yVs5tABEBAAG0AIkBHAQQAQIABgUCV1rR
JQAKCRARRr1AwSSieORICAC2+YDCX2hXVU8bCg8MxluKTTu+k5PIZAcb7jPO4nGB
g/Ltg3luWuJ5YM0oRnSfRikIe8QI7SxnjfnIig58lhbT/raHFCv4LleMhqCsKOZT
ikwDanD67vlmIOvGVbLk+cEbxjTMGb3O8DzFzxkJfoj+g2XdwPTKyxpr/g9b8YAB
GDpQjznrnJ3Ei6aeYuXjNKOVhEZTpAldWFCVh40ABgsZ4zsv3pICbjScCEBdM0Af
jsg0HMRLT0pw3aL6aFv2355yiNg9LKvOg31pkBw/N49W3kTeFGRt6HxTqWnlE3e0
Ml806J728vVeyeOJygHrcThQGAQGLbwyY9bg7Oc5CF/L
=nyy+
-----END PGP PUBLIC KEY BLOCK-----"

let base64pgp (pgp: string) =
  pgp
  |> Encoding.UTF8.GetBytes
  |> Convert.ToBase64String

let initRequest: RekeyInitRequest =
  { secretShares = 1
    secretTreshold = 1
    pgpKeys = None//Some [base64pgp pgpKey]
    backup = None//Some true
  }

[<Tests>]
let tests =
  let nonce =
    memo <| job {
      let! initState = initState
      let listing = Keys.rekeyInit initState (initRequest, [])
      return! ensureSuccess listing <| fun (resp) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Nonce: %s PGPFIngerPrints: %A" resp.nonce resp.pgpFIngerPrints)
        resp.nonce
    }

  testList "Vault Keys tests" [
    testCaseAsync "sys.keysStatus ->  information about the current encryption key " <| async {
      let! initState = initState
      let listing = Keys.keyStatus initState []
      do! ensureSuccess listing <| fun (status) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Time: %s Term: %i" status.installTime status.term)
    }

    testCaseAsync "sys.rotate ->  rotation of the backend encryption key " <| async {
      let! initState = initState
      let listing = Keys.rotate initState []
      do! ensureSuccess listing <| fun (status) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Encryption key rotated.")
    }

    testCaseAsync "sys.rekeyStatus ->  progress of the current rekey attempt" <| async {
      let! initState = initState
      let listing = Keys.rekeyStatus initState []
      do! ensureSuccess listing <| fun (resp) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Nonce: %s PGPFIngerPrints: %A" resp.nonce resp.pgpFIngerPrints)
    }

    testCaseAsync "sys.rekeyInit ->  new rekey attempt" <| async {
      let! n = nonce
      do ignore n
    }

    testCaseAsync "sys.rekeyUpdate ->  new rekey attempt" <| async {
      let! initState = initState
      let! nonce = nonce
      let m =
        Map [
          "nonce", nonce
          "key", initState.config.keys.Value.[0]
        ]
      let listing = Keys.rekeyUpdate initState (m, [])
      do! ensureSuccess listing <| fun (resp) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Nonce: %s PGPFIngerPrints: %A" resp.nonce resp.pgpFIngerPrints)
    }

    testCaseAsync "sys.rekeyCancel ->  cancel rekey attempts" <| async {
      let! initState = initState
      let listing = Keys.rekeyCancel initState []
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Rekey operation cancelled")
    }

//    testCase "sys.rekeyBackupRetrieve ->  return the backup copy of PGP-encrypted unseal keys" <| fun _ ->
//      let listing = Keys.RekeyRetrieveBackup initState []
//      ensureSuccess listing <| fun backup ->
//        let logger = state.logger
//        logger.logSimple (Message.sprintf Debug "Backup retrieved: %s, %A" backup.Nonce backup.Keys)

    testCaseAsync "sys.rekeyBackupRetrieve ->  delete the backup copy of PGP-encrypted unseal keys" <| async {
      let! initState = initState
      let listing = Keys.rekeyDeleteBackup initState []
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Backup deleted")
    }

/// 500 - unsupported path
//    testCase "sys.rekeyRecoveryStatus ->  progress of the current rekey attempt" <| fun _ ->
//      let listing = Keys.RekeyRecoveryKeyStatus initState []
//      ensureSuccess listing <| fun (resp) ->
//        let logger = state.logger
//        logger.logSimple (Message.sprintf Debug "Nonce: %s PGPFIngerPrints: %A" resp.Nonce resp.PGPFIngerPrints)
//
//    testCase "sys.rekeyRecoveryInit ->  new rekey attempt" <| fun _ ->
//      let listing = Keys.RekeyRecoveryKeyInit initState (initRequest, [])
//      ensureSuccess listing <| fun (resp) ->
//        let logger = state.logger
//        logger.logSimple (Message.sprintf Debug "Nonce: %s PGPFIngerPrints: %A" resp.Nonce resp.PGPFIngerPrints)
//
//    testCase "sys.rekeyRecoveryUpdate ->  new rekey attempt" <| fun _ ->
//      let listing = Keys.RekeyRecoveryKeyUpdate initState ((Map.empty.Add("nonce", nonce).Add("key", initState.config.keys.Value.[0])), [])
//      ensureSuccess listing <| fun (resp) ->
//        let logger = state.logger
//        logger.logSimple (Message.sprintf Debug "Nonce: %s PGPFIngerPrints: %A" resp.Nonce resp.PGPFIngerPrints)
//
//    testCase "sys.rekeyRecoveryCancel ->  cancel rekey attempts" <| fun _ ->
//      let listing = Keys.RekeyRecoveryKeyCancel initState []
//      ensureSuccess listing <| fun _ ->
//        let logger = state.logger
//        logger.logSimple (Message.sprintf Debug "Rekey operation cancelled")
//
//    testCase "sys.rekeyRecoveryBackupRetrieve ->  delete the backup copy of PGP-encrypted recovery unseal keys" <| fun _ ->
//      let listing = Keys.RekeyDeleteRecoveryBackup initState []
//      ensureSuccess listing <| fun _ ->
//        let logger = state.logger
//        logger.logSimple (Message.sprintf Debug "Backup deleted")

]