module Fakta.IntegrationTests.Mounts

open Expecto
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
    testCaseAsync "sys.mounts.mount -> Mount a new secret backend" <| async {
      let! initState = initState
      let listing = Mounts.mount initState ((mountPointName, consulMountInput),[])
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Mountpoint created.")
    }

    testCaseAsync "sys.mounts.mount -> Mount second secret backend" <| async {
      let! initState = initState
      let listing = Mounts.mount initState (("consul", consulMountInput),[])
      do! ensureSuccess listing <| fun resp ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Mountpoint created.")
    }

    testCaseAsync "sys.mounts.mount -> Mount pki secret backend" <| async {
      let! initState = initState
      let listing = Mounts.mount initState (("pki", pkiMountInput),[])
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Mountpoint created.")
    }

    testCaseAsync "sys.mounts.mounts -> list of the mounted secret backends." <| async {
      let! initState = initState
      let listing = Mounts.mounts initState []
      do! ensureSuccess listing <| fun mounts ->
        let logger = state.logger
        for KeyValue (name, mount)  in mounts do
          logger.logSimple (Message.sprintf Debug "Mount name: %s, description: %s" name mount.description)
    }

    testCaseAsync "sys.mounts.tuneMount -> tune configuration parameters for a given mount point" <| async {
      let! initState = initState
      let listing = Mounts.tuneMount initState ((mountPointName, mountConfig), [])
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "%s mounpoint tuned." mountPointName)
    }

    testCaseAsync "sys.mounts.tuneMount -> tune configuration parameters for pki mount point" <| async {
      let! initState = initState
      let listing = Mounts.tuneMount initState (("pki", mountConfig), [])
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "PKI mounpoint tuned.")
    }

    testCaseAsync "sys.mounts.remount -> remount an already-mounted backend to a new mount point." <| async {
      let! initState = initState
      let listing = Mounts.remount initState ((mountPointName, "test"), [])
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Mounpoint remounted.")
    }

    testCaseAsync "sys.mounts.unmount -> remount an already-mounted backend to a new mount point." <| async {
      let! initState = initState
      let listing = Mounts.unmount initState ("test", [])
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Mounpoint unmounted.")
    }
]