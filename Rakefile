require 'bundler/setup'

require 'albacore'
require 'albacore/tasks/release'
require 'albacore/tasks/versionizer'
require 'albacore/ext/teamcity'

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
build :compile => [:versioning, :restore, :assembly_info] do |b|
  b.prop 'Configuration', Configuration
  b.sln = 'src/Fakta.sln'
end

directory 'build/pkg'

desc 'package nugets - finds all projects and package them'
nugets_pack :create_nugets => ['build/pkg', :versioning, :compile] do |p|
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

namespace :tests do
  task :unit do
    system "src/Fakta.Tests/bin/#{Configuration}/Fakta.Tests.exe",
           clr_command: true
  end
end

task :tests => :'tests:unit'

task :default => [:compile, :tests, :create_nugets]

task :ensure_nuget_key do
  raise 'missing env NUGET_KEY value' unless ENV['NUGET_KEY']
end

Albacore::Tasks::Release.new :release,
                             pkg_dir: 'build/pkg',
                             depend_on: [:create_nugets, :ensure_nuget_key],
                             nuget_exe: 'packages/NuGet.CommandLine/tools/NuGet.exe',
                             api_key: ENV['NUGET_KEY']
