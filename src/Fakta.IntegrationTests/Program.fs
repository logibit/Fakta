module Fakta.IntegrationTests.Program

open Fuchu

[<EntryPoint>]
let main argv = 
  //Tests.defaultMainThisAssembly argv
  
  // I don't know how else to make tests to run in the exact order
  Tests.run Agent.tests  |> ignore
  Tests.run ACL.tests |> ignore
  Tests.run Catalog.tests  |> ignore
  Tests.run Event.tests  |> ignore
  Tests.run Health.tests  |> ignore
  Tests.run KV.tests  |> ignore
  Tests.run Session.tests  |> ignore
  Tests.run Status.tests  |> ignore

  Tests.run Init.tests |> ignore
  Tests.run Seal.tests |> ignore
  Tests.run GenerateRoot.tests |> ignore
  Tests.run Mounts.tests |> ignore
  Tests.run Leader.tests |> ignore
  Tests.run Stepdown.tests |> ignore
  Tests.run Keys.tests |> ignore
  Tests.run Secrets.testsGeneric |> ignore
  Tests.run Audit.tests |> ignore
  Tests.run HealthVault.tests |> ignore
  Tests.run Auth.tests  |> ignore
  Tests.run Policy.tests |> ignore
  Tests.run Capabilities.tests
 
