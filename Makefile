PACKAGE_ROOT := $(shell echo $(USERPROFILE))\.nuget\packages
COMPILER := msbuild

# TDD tools and related variables
TDD_TOOL := $(PACKAGE_ROOT)\nunit.consolerunner\3.10.0\tools\nunit3-console.exe
TDD_DIR := .\OpenCover

COVERAGE_TOOL := $(PACKAGE_ROOT)\opencover\4.7.922\tools\OpenCover.Console.exe
COVERAGE_REPORT_TOOL := $(PACKAGE_ROOT)\reportgenerator\4.4.7\tools\net47\ReportGenerator.exe
COVERAGE_REPORT := $(TDD_DIR)\results.xml

TESTS = .\VM.Tests\bin\$(CONFIG)\VM.Tests.dll

OPENCOVER_ASSEMBLY_FILTER := -nunit.framework;-VM.Tests;

# Version and Git related variables
GIT_LONG_HASH := $(shell git rev-parse HEAD)
GIT_SHORT_HASH := $(shell git rev-parse --short HEAD)

# Solution Information
SOLUTION := 16-Bit-Virtual-Machine
SOLUTION_FILE := .\$(SOLUTION).sln

# Build output variables
RELEASE_DIR := .\Release

ARTIFACTS_DIR := .\artifacts

# default (debug) values (overridden during non-debug builds)
CONFIG := Debug

###############################
# Utility functions
###############################
# Deletes directory if it exists
# $1 Directory to delete
define delete_dir
	@if EXIST $1 rmdir $1 /s /q;
endef

# Creates a directory if it does not exist
# $! Directory to create
define make_dir
	@if NOT EXIST $1 mkdir $1;
endef

# Copies all output files for a specified project to $(RELEASE_DIR)
# DLLs get copied to their own folder as well as the $(RELEASE_DIR)/All folder
# $1 Name of project to copy files for
define copy_dll_to_release
	$(call copy_to_folder,$1,$(RELEASE_DIR)\$1)
	$(call copy_to_folder,$1,$(RELEASE_DIR)\All)
endef

# Copies all output files for a specified project to $(RELEASE_DIR)
# $1 Name of project to copy files for
define copy_exe_to_release
	$(call copy_to_folder,$1,$(RELEASE_DIR)\$1)
endef

# Copies contents of bin\<CONFIG> folder to another folder
# $1 project name
# $2 is folder to copy to
define copy_to_folder
	@echo $1 ^> $2
	@(robocopy .\$1\bin\$(CONFIG) $2 /S /NFL /NDL /NJH /NJS /NC /NS /NP) ^& if %ERRORLEVEL% leq 1 exit 0
endef

###############################
# Make rules
###############################
# Default rule.
.PHONY: all
all: tdd

# This rule builds the solution
.PHONY: build
build:
	@echo _
	@echo -----------------------------------
	@echo Building Solution ($(CONFIG)) ...
	@echo -----------------------------------
	$(COMPILER) $(SOLUTION_FILE) -v:m -nologo -t:Rebuild -p:Configuration=$(CONFIG) -restore -m -nr:False

# This rule runs nunit and coverage
.PHONY: tdd
tdd: build
	@echo _
	@echo -----------------------------------
	@echo Running TDD tests w/ coverage ...
	@echo -----------------------------------
	$(call delete_dir,$(TDD_DIR))
	$(call make_dir,$(TDD_DIR))
	$(COVERAGE_TOOL) -target:$(TDD_TOOL) -targetargs:"$(TESTS) --work=$(TDD_DIR)" -register:user -output:$(COVERAGE_REPORT)
	$(COVERAGE_REPORT_TOOL) -reports:$(COVERAGE_REPORT) -targetdir:$(TDD_DIR) -assemblyFilters:$(OPENCOVER_ASSEMBLY_FILTER) -verbosity:Warning -tag:$(GIT_LONG_HASH)

# This rule copies build output files to $(RELEASE_DIR)
# NOTE: this will be Release or Debug binaries depending on build configuration
.PHONY: package
package: tdd
	@echo _
	@echo -----------------------------------
	@echo Copying to $(RELEASE_DIR) ...
	@echo -----------------------------------
	$(call copy_dll_to_release,VM)

# Builds a release build.
.PHONY: release
release: CONFIG := Release
release: package

# Builds a debug build.
.PHONY: debug
debug: tdd

# This rule cleans the project (removes binaries, etc).
.PHONY: clean
clean:
	@echo _
	@echo -----------------------------------
	@echo Cleaning solution ...
	@echo -----------------------------------
	$(call delete_dir,$(RELEASE_DIR))
	$(COMPILER) $(SOLUTION_FILE) -v:m -nologo -t:Clean -p:Configuration=Debug -m -nr:False
	$(COMPILER) $(SOLUTION_FILE) -v:m -nologo -t:Clean -p:Configuration=Release -m -nr:False