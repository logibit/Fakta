module Fakta.IntegrationTests.Mounts

open System
open System.Net
open Chiron
open Chiron.Operators
open Fuchu
open NodaTime
open Fakta
open Fakta.Logging
open Fakta.Vault

let mountConfig: MountConfigInput = { DefaultLeaseTTL = "0"
                                      MaxLeaseTTL = "0"}

let mountInput: MountInput =
            { ``Type`` = "consul"
              Description = "test consul mountpoint"
              Config = Some mountConfig}
let mountPointName = "consulTest"

[<Tests>]
let tests =
  testList "Vault mounts tests" [
    testCase "sys.mounts.mount -> Mount a new secret backend" <| fun _ ->
      let listing = Mounts.Mount initState ((mountPointName, mountInput),[])
      ensureSuccess listing <| fun resp ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Mountpoint created.")

    testCase "sys.mounts.mount -> Mount second secret backend" <| fun _ ->
      let listing = Mounts.Mount initState (("consul", mountInput),[])
      ensureSuccess listing <| fun resp ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Mountpoint created.")

    testCase "sys.mounts.mounts -> list of the mounted secret backends." <| fun _ ->
      let listing = Mounts.Mounts initState []
      ensureSuccess listing <| fun (mounts) ->
        let logger = state.logger
        for KeyValue (name, mount)  in mounts do
          logger.logSimple (Message.sprintf [] "Mount name: %s, description: %s" name mount.Description)

    testCase "sys.mounts.tuneMount -> tune configuration parameters for a given mount point" <| fun _ ->
      let listing = Mounts.TuneMount initState ((mountPointName, mountConfig), [])
      ensureSuccess listing <| fun resp ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Mounpoint tuned.")

    testCase "sys.mounts.remount -> remount an already-mounted backend to a new mount point." <| fun _ ->
      let listing = Mounts.Remount initState ((mountPointName, "test"), [])
      ensureSuccess listing <| fun resp ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Mounpoint remounted.")

    testCase "sys.mounts.unmount -> remount an already-mounted backend to a new mount point." <| fun _ ->
      let listing = Mounts.Unmount initState ("test", [])
      ensureSuccess listing <| fun resp ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Mounpoint unmounted.")
]