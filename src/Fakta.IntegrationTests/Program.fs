module Fakta.IntegrationTests.Program

open Fuchu

[<EntryPoint>]
let main argv = 
  //Tests.defaultMainThisAssembly argv
  Tests.run Agent.tests
  //Tests.run Status.tests
  //Tests.run ACL.tests