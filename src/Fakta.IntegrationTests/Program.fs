module Fakta.IntegrationTests.Program

open Fuchu

[<EntryPoint>]
let main argv = 
  //Tests.defaultMainThisAssembly argv
  //Tests.run Agent.tests
  Tests.run ACL.tests |> ignore
  //Tests.run Catalog.tests
  //Tests.run Event.tests
  //Tests.run Health.tests
  //Tests.run KV.tests
  //Tests.run Session.tests
  //Tests.run Status.tests

  Tests.run Init.tests |> ignore
  Tests.run Seal.tests |> ignore
  Tests.run GenerateRoot.tests |> ignore
  Tests.run Mounts.tests |> ignore
  Tests.run Leader.tests |> ignore
  Tests.run Stepdown.tests |> ignore
  Tests.run Keys.tests |> ignore
  Tests.run Secrets.testsGeneric |> ignore
  //Tests.run Secrets.testsConsul
  Tests.run Audit.tests
 
