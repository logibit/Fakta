module Fakta.IntegrationTests.Mounts

open Fuchu
open Fakta
open Fakta.Logging
open Fakta.Vault

let mountConfig: MountConfigInput = 
  { defaultLeaseTTL = "0"
    maxLeaseTTL = "87600h"}

let consulMountInput: MountInput =
  { ``type`` = "consul"
    description = "test consul mountpoint"
    mountConfig = Some mountConfig}
let pkiMountInput: MountInput =
  { ``type`` = "pki"
    description = "pki mountpoint"
    mountConfig = Some mountConfig}
let mountPointName = "consulTest"

[<Tests>]
let tests =
  testList "Vault mounts tests" [
    testCase "sys.mounts.mount -> Mount a new secret backend" <| fun _ ->
      let listing = Mounts.mount initState ((mountPointName, consulMountInput),[])
      ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Mountpoint created.")

    testCase "sys.mounts.mount -> Mount second secret backend" <| fun _ ->
      let listing = Mounts.mount initState (("consul", consulMountInput),[])
      ensureSuccess listing <| fun resp ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Mountpoint created.")

    testCase "sys.mounts.mount -> Mount pki secret backend" <| fun _ ->
      let listing = Mounts.mount initState (("pki", pkiMountInput),[])
      ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Mountpoint created.")

    testCase "sys.mounts.mounts -> list of the mounted secret backends." <| fun _ ->
      let listing = Mounts.mounts initState []
      ensureSuccess listing <| fun mounts ->
        let logger = state.logger
        for KeyValue (name, mount)  in mounts do
          logger.logSimple (Message.sprintf Debug "Mount name: %s, description: %s" name mount.description)

    testCase "sys.mounts.tuneMount -> tune configuration parameters for a given mount point" <| fun _ ->
      let listing = Mounts.tuneMount initState ((mountPointName, mountConfig), [])
      ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "%s mounpoint tuned." mountPointName)

    testCase "sys.mounts.tuneMount -> tune configuration parameters for pki mount point" <| fun _ ->
      let listing = Mounts.tuneMount initState (("pki", mountConfig), [])
      ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "PKI mounpoint tuned.")

    testCase "sys.mounts.remount -> remount an already-mounted backend to a new mount point." <| fun _ ->
      let listing = Mounts.remount initState ((mountPointName, "test"), [])
      ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Mounpoint remounted.")

    testCase "sys.mounts.unmount -> remount an already-mounted backend to a new mount point." <| fun _ ->
      let listing = Mounts.unmount initState ("test", [])
      ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Mounpoint unmounted.")
]