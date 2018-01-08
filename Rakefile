require 'bundler/setup'
require 'albacore'
require 'albacore/tasks/release'
require 'albacore/tasks/versionizer'

Configuration = ENV['CONFIGURATION'] || 'Release'

Albacore::Tasks::Versionizer.new :versioning

desc 'create assembly infos'
asmver_files :assembly_info do |a|
  a.files = FileList['src/**/*proj'] # optional, will find all projects recursively by default

  a.attributes assembly_description: 'HTTP API for Consul in F#',
               assembly_configuration: Configuration,
               assembly_company: 'Logibit AB',
               assembly_copyright: "(c) 2015 by Henrik Feldt",
               assembly_version: ENV['LONG_VERSION'],
               assembly_file_version: ENV['LONG_VERSION'],
               assembly_informational_version: ENV['BUILD_VERSION']
end

task :yolo do
  sh %{ruby -pi.bak -e \
        "gsub(/namespace Logary.Facade/, 'namespace Fakta.Logging')" \
         paket-files/logary/logary/src/Logary.Facade/Facade.fs}
  sh %{ruby -pi.bak -e \
         "gsub(/module internal YoLo/, 'module internal Fakta.YoLo')" \
         paket-files/haf/YoLo/YoLo.fs}
end

desc 'Perform fast build (warn: doesn\'t d/l deps)'
build :quick_compile do |b|
  b.prop 'Configuration', Configuration
  b.logging = 'detailed'
  b.sln     = 'src/Fakta.sln'
end

task :paket_bootstrap do
system 'tools/paket.bootstrapper.exe', clr_command: true unless   File.exists? 'tools/paket.exe'
end

desc 'restore all nugets as per the packages.config files'
task :restore => :paket_bootstrap do
  system 'tools/paket.exe', 'restore', clr_command: true
end

desc 'Perform full build'
build :compile => [:versioning, :restore, :assembly_info, :yolo] do |b|
  b.prop 'Configuration', Configuration
  b.sln = 'src/Fakta.sln'
end

directory 'build/pkg'

desc 'package nugets - finds all projects and package them'
nugets_pack :create_nugets_quick do |p|
  p.configuration = Configuration
  p.files   = FileList['src/**/*.{csproj,fsproj,nuspec}'].
    exclude(/Tests/)
  p.out     = 'build/pkg'
  p.exe     = 'packages/NuGet.CommandLine/tools/NuGet.exe'
  p.with_metadata do |m|
    m.title       = 'Fakta - Consul API'
    m.description = 'Fakta is a Consul HTTP API in F#'
    m.authors     = 'Henrik Feldt, Logibit AB'
    m.project_url = 'http://github.com/logibit/Fakta'
    m.tags        = 'consul http api fsharp environment env 12factor'
    m.version     = ENV['NUGET_VERSION']
  end
end

task :create_nugets => ['build/pkg', :versioning, :compile]

namespace :tests do
  task :unit do
    system "src/Fakta.Tests/bin/#{Configuration}/Fakta.Tests.exe",
           clr_command: true
  end

  task :integration do
    system "src/Fakta.IntegrationTests/bin/#{Configuration}/Fakta.IntegrationTests.exe",
           clr_command: true
  end
end

task :tests => [:'tests:unit', :'tests:integration']

task :default => [:compile, :tests, :create_nugets]

task :ensure_nuget_key do
  raise 'missing env NUGET_KEY value' unless ENV['NUGET_KEY']
end

Albacore::Tasks::Release.new :release_quick,
                             pkg_dir: 'build/pkg',
                             depend_on: [:ensure_nuget_key],
                             nuget_exe: 'packages/NuGet.CommandLine/tools/NuGet.exe',
                             api_key: ENV['NUGET_KEY']

task :release => [ :tests, :create_nugets, :release_quick ]
