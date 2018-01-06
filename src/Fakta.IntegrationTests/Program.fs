module Fakta.IntegrationTests.Program

open Expecto

[<EntryPoint>]
let main argv =
  Tests.runTestsWithArgs defaultConfig argv <|
    testList "fakta" [
      testSequencedGroup "consul" (
        testList "consul" [
          Agent.tests
          ACL.tests
          Catalog.tests
          Event.tests
          Health.tests
          KV.tests
          Session.tests
          Status.tests
        ]
      )
      testSequencedGroup "vault" (
        testList "vault" [
          Init.tests
          Seal.tests
          GenerateRoot.tests
          Mounts.tests
          Leader.tests
          Stepdown.tests
          Keys.tests
          Secrets.testsGeneric
          Secrets.testsConsul 
          Audit.tests
          HealthVault.tests
          Auth.tests 
          Policy.tests
          Capabilities.tests
        ]
      )
    ]