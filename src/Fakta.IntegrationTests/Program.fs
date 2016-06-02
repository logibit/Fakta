module Fakta.IntegrationTests.Program

open Fuchu

[<EntryPoint>]
let main argv = 
  //Tests.defaultMainThisAssembly argv
  ///Tests.run Agent.tests
  //Tests.run ACL.tests
  Tests.run Catalog.tests
  //Tests.run Event.tests
  //Tests.run Health.tests
  //Tests.run KV.tests
  //Tests.run Session.tests
  //Tests.run Status.tests
 
