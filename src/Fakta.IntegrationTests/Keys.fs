module Fakta.IntegrationTests.Keys

open System
open System.Net
open Chiron
open Chiron.Operators
open Fuchu
open NodaTime
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

let base64pgp (pgp : string) =
    pgp
    |> Encoding.UTF8.GetBytes
    |> Convert.ToBase64String
    

let initRequest: RekeyInitRequest =
  { SecretShares = 1
    SecretTreshold = 1
    PGPKeys = None//Some [base64pgp pgpKey]
    Backup = None//Some true
    }



[<Tests>]
let tests =
  let nonce =
    let listing = Keys.RekeyInit initState (initRequest, [])
    ensureSuccess listing <| fun (resp) ->
      let logger = state.logger
      logger.logSimple (Message.sprintf [] "Nonce: %s PGPFIngerPrints: %A" resp.Nonce resp.PGPFIngerPrints)
      resp.Nonce

  testList "Vault Keys tests" [
    testCase "sys.keysStatus ->  information about the current encryption key " <| fun _ ->
      let listing = Keys.KeyStatus initState []
      ensureSuccess listing <| fun (status) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Time: %s Term: %i" status.InstallTime status.Term)

    testCase "sys.rotate ->  rotation of the backend encryption key " <| fun _ ->
        let listing = Keys.Rotate initState []
        ensureSuccess listing <| fun (status) ->
          let logger = state.logger
          logger.logSimple (Message.sprintf [] "Encryption key rotated.")

    testCase "sys.rekeyStatus ->  progress of the current rekey attempt" <| fun _ ->
      let listing = Keys.RekeyStatus initState []
      ensureSuccess listing <| fun (resp) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Nonce: %s PGPFIngerPrints: %A" resp.Nonce resp.PGPFIngerPrints)

    testCase "sys.rekeyInit ->  new rekey attempt" <| fun _ ->
      nonce |> ignore

    testCase "sys.rekeyUpdate ->  new rekey attempt" <| fun _ ->
      let listing = Keys.RekeyUpdate initState ((Map.empty.Add("nonce", nonce).Add("key", initState.config.keys.Value.[0])), [])
      ensureSuccess listing <| fun (resp) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Nonce: %s PGPFIngerPrints: %A" resp.Nonce resp.PGPFIngerPrints)

    testCase "sys.rekeyCancel ->  cancel rekey attempts" <| fun _ ->
      let listing = Keys.RekeyCancel initState []
      ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Rekey operation cancelled")

//    testCase "sys.rekeyBackupRetrieve ->  return the backup copy of PGP-encrypted unseal keys" <| fun _ ->
//      let listing = Keys.RekeyRetrieveBackup initState []
//      ensureSuccess listing <| fun backup ->
//        let logger = state.logger
//        logger.logSimple (Message.sprintf [] "Backup retrieved: %s, %A" backup.Nonce backup.Keys)

    testCase "sys.rekeyBackupRetrieve ->  delete the backup copy of PGP-encrypted unseal keys" <| fun _ ->
      let listing = Keys.RekeyDeleteBackup initState []
      ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Backup deleted")

/// 500 - unsupported path
//    testCase "sys.rekeyRecoveryStatus ->  progress of the current rekey attempt" <| fun _ ->
//      let listing = Keys.RekeyRecoveryKeyStatus initState []
//      ensureSuccess listing <| fun (resp) ->
//        let logger = state.logger
//        logger.logSimple (Message.sprintf [] "Nonce: %s PGPFIngerPrints: %A" resp.Nonce resp.PGPFIngerPrints)
//
//    testCase "sys.rekeyRecoveryInit ->  new rekey attempt" <| fun _ ->
//      let listing = Keys.RekeyRecoveryKeyInit initState (initRequest, [])
//      ensureSuccess listing <| fun (resp) ->
//        let logger = state.logger
//        logger.logSimple (Message.sprintf [] "Nonce: %s PGPFIngerPrints: %A" resp.Nonce resp.PGPFIngerPrints)
//
//    testCase "sys.rekeyRecoveryUpdate ->  new rekey attempt" <| fun _ ->
//      let listing = Keys.RekeyRecoveryKeyUpdate initState ((Map.empty.Add("nonce", nonce).Add("key", initState.config.keys.Value.[0])), [])
//      ensureSuccess listing <| fun (resp) ->
//        let logger = state.logger
//        logger.logSimple (Message.sprintf [] "Nonce: %s PGPFIngerPrints: %A" resp.Nonce resp.PGPFIngerPrints)
//
//    testCase "sys.rekeyRecoveryCancel ->  cancel rekey attempts" <| fun _ ->
//      let listing = Keys.RekeyRecoveryKeyCancel initState []
//      ensureSuccess listing <| fun _ ->
//        let logger = state.logger
//        logger.logSimple (Message.sprintf [] "Rekey operation cancelled")
//
//    testCase "sys.rekeyRecoveryBackupRetrieve ->  delete the backup copy of PGP-encrypted recovery unseal keys" <| fun _ ->
//      let listing = Keys.RekeyDeleteRecoveryBackup initState []
//      ensureSuccess listing <| fun _ ->
//        let logger = state.logger
//        logger.logSimple (Message.sprintf [] "Backup deleted")

]