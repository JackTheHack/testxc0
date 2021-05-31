using System.Text.RegularExpressions;

/*===============================================
================= HELPER METHODS ================
===============================================*/

public class Configuration
{
    private MSBuildToolVersion _msBuildToolVersion;    

    public string ProjectName {get;set;}
    public string XConnectRoot {get;set;}
    public string InstanceUrl {get;set;}
    public string SolutionName {get;set;}
    public string ProjectFolder {get;set;}
    public string ProjectBuildFolder {get;set;}
    public string BuildConfiguration {get;set;}
    public string MessageStatisticsApiKey {get;set;}
    public string MarketingDefinitionsApiKey {get;set;}
    public bool RunCleanBuilds {get;set;}
	public int DeployExmTimeout {get;set;}
    public string DeployFolder {get;set;}      
    public string Version {get;set;}
    public bool CDN {get;set;}
    public string DeploymentTarget{get;set;}
    
    public string BuildToolVersions 
    {
        set 
        {
            if(!Enum.TryParse(value, out this._msBuildToolVersion))
            {
                this._msBuildToolVersion = MSBuildToolVersion.Default;
            }
        }
    }

    public string SourceFolder => $"{ProjectFolder}\\src";    
    public string UnicornSourceFolder => $"{ProjectFolder}\\artifacts\\sitecore";
    public string FoundationSrcFolder => $"{SourceFolder}\\Foundation";
    public string FeatureSrcFolder => $"{SourceFolder}\\Feature";
    public string ProjectSrcFolder => $"{SourceFolder}\\Project";

    public string SolutionFile => $"{ProjectFolder}\\{SolutionName}";
    public MSBuildToolVersion MSBuildToolVersion => this._msBuildToolVersion;
    public string BuildTargets => this.RunCleanBuilds ? "Clean;Build" : "Build";
}

public void PrintHeader(ConsoleColor foregroundColor)
{
    cakeConsole.ForegroundColor = foregroundColor;
    cakeConsole.WriteLine("     "); 
    cakeConsole.WriteLine("     "); 
    cakeConsole.WriteLine(@" --------------------  ------------------");
    cakeConsole.WriteLine("   " + "Building the project");
    
    cakeConsole.WriteLine("     "); 
    cakeConsole.WriteLine("     ");
    cakeConsole.ResetColor();
}

public void PublishProjects(string rootFolder, string publishRoot, List<string> projectsToInclude = null)
{
    var projects = GetFiles($"{rootFolder}\\**\\code\\*.csproj");    
    if(projectsToInclude != null && projectsToInclude.Any()){
        projectsToInclude = projectsToInclude.Select(p=> p.ToLower()).ToList();
        projects = new FilePathCollection(projects.Where(p=> projectsToInclude.Contains(p.GetFilenameWithoutExtension().ToString().ToLower())));
    }
    Information("Publishing " + rootFolder + " to " + publishRoot);
    var excludeConfigTransform = configuration.DeploymentTarget != "Azure";
    foreach (var project in projects)
    {        
        MSBuild(project, cfg => InitializeMSBuildSettings(cfg)
                                   .WithTarget(configuration.BuildTargets)
                                   .WithProperty("Configuration", configuration.BuildConfiguration)
                                   .WithProperty("MarkWebConfigAssistFilesAsExclude", $"{excludeConfigTransform}")
                                   .WithProperty("DeployOnBuild", "true")
                                   .WithProperty("DeployDefaultTarget", "WebPublish")
                                   .WithProperty("WebPublishMethod", "FileSystem")
                                   .WithProperty("DeleteExistingFiles", "false")
                                   .WithProperty("publishUrl", publishRoot)
                                   .WithProperty("BuildProjectReferences", "false")
                                   );
    }
}

public void PublishProject(string projectPath, string publishRoot)
{
    Information("Publishing " + projectPath + " to " + publishRoot);
    MSBuild(projectPath, cfg => InitializeMSBuildSettings(cfg)
                                   .WithTarget(configuration.BuildTargets)
                                   .WithProperty("Configuration", configuration.BuildConfiguration)
                                   .WithProperty("MarkWebConfigAssistFilesAsExclude", "false")
                                   .WithProperty("DeployOnBuild", "true")
                                   .WithProperty("DeployDefaultTarget", "WebPublish")
                                   .WithProperty("WebPublishMethod", "FileSystem")
                                   .WithProperty("DeleteExistingFiles", "false")
                                   .WithProperty("publishUrl", publishRoot)
                                   .WithProperty("BuildProjectReferences", "false")
                                   );
}

public FilePathCollection GetTransformFiles(string rootFolder)
{
    Func<IFileSystemInfo, bool> exclude_obj_bin_folder =fileSystemInfo => !fileSystemInfo.Path.FullPath.Contains("/obj/") || !fileSystemInfo.Path.FullPath.Contains("/bin/");

    var xdtFiles = GetFiles($"{rootFolder}\\**\\*.xdt", exclude_obj_bin_folder);

    return xdtFiles;
}

public void Transform(string rootFolder) {
    var xdtFiles = GetTransformFiles(rootFolder);

    foreach (var file in xdtFiles)
    {
        if (file.FullPath.ToLower().Contains(".azure"))
        {
            continue;
        }
        
        Information($"Applying configuration transform:{file.FullPath}");
        var fileToTransform = Regex.Replace(file.FullPath.ToLower(), ".+code/(.+)/*.xdt", "$1");
        fileToTransform = Regex.Replace(fileToTransform.ToLower(), ".sc-internal", "");
        fileToTransform = Regex.Replace(fileToTransform.ToLower(), ".common", "");
        //var sourceTransform = $"{configuration.WebsiteRoot}\\{fileToTransform}";
        
        // XdtTransformConfig(sourceTransform			                // Source File
        //                     , file.FullPath			                // Tranforms file (*.xdt)
        //                     , sourceTransform);		                // Target File
    }
}

public void RebuildIndex(string indexName)
{
    var url = $"{configuration.InstanceUrl}utilities/indexrebuild.aspx?index={indexName}";
    string responseBody = HttpGet(url);
}

public void DeployExmCampaigns()
{
	var url = $"{configuration.InstanceUrl}utilities/deployemailcampaigns.aspx?apiKey={configuration.MessageStatisticsApiKey}";
	var responseBody = HttpGet(url, settings =>
	{
		settings.AppendHeader("Connection", "keep-alive");
	});

    Information(responseBody);
}

public MSBuildSettings InitializeMSBuildSettings(MSBuildSettings settings)
{
    settings.SetConfiguration(configuration.BuildConfiguration)
            .SetVerbosity(Verbosity.Minimal)
            .SetMSBuildPlatform(MSBuildPlatform.Automatic)
            .SetPlatformTarget(PlatformTarget.MSIL)
            .UseToolVersion(configuration.MSBuildToolVersion)
            .WithRestore();
    return settings;
}

public void CreateFolder(string folderPath)
{
    if (!DirectoryExists(folderPath))
    {
        CreateDirectory(folderPath);
    }
}

public void Spam(Action action, int? timeoutMinutes = null)
{
	Exception lastException = null;
	var startTime = DateTime.Now;
	while (timeoutMinutes == null || (DateTime.Now - startTime).TotalMinutes < timeoutMinutes)
	{
		try {
			action();

			Information($"Completed in {(DateTime.Now - startTime).Minutes} min {(DateTime.Now - startTime).Seconds} sec.");
			return;
		} catch (AggregateException aex) {
		    foreach (var x in aex.InnerExceptions)
				Information($"{x.GetType().FullName}: {x.Message}");
			lastException = aex;
		} catch (Exception ex) {
		    Information($"{ex.GetType().FullName}: {ex.Message}");
			lastException = ex;
		}
	}

    throw new TimeoutException($"Unable to complete within {timeoutMinutes} minutes.", lastException);
}

public void WriteError(string errorMessage)
{
    cakeConsole.ForegroundColor = ConsoleColor.Red;
    cakeConsole.WriteError(errorMessage);
    cakeConsole.ResetColor();
}

public void UpdateCDRole(string path, string findText, string replaceText)
{
   string text = System.IO.File.ReadAllText(path);
   System.IO.File.WriteAllText(path, text.Replace(findText,replaceText));
}
public void DeleteBinDll(string path, string pattern)
{
var files = System.IO.Directory.GetFiles(path,pattern);
            foreach (var file in files)
            {
                System.IO.File.Delete(file);
               
            }
}

private void ApplyXmlTransformsForCMS(string xdtFileNamePattern, string sourceFolderPath){
  var xdtFilePaths = GetPaths($"{sourceFolderPath}/**/{xdtFileNamePattern}").ToList().OrderBy(x=>x.FullPath, new HelixFileNameComparer());  
  var fileExt = ".config";
  foreach (var xdtFilePath in xdtFilePaths)
  {
    var targetFilePath = new FilePath(xdtFilePath.FullPath.Substring(0, xdtFilePath.FullPath.IndexOf(fileExt) + fileExt.Count()));
    var xdtFileInfo = new FileInfo(xdtFilePath.FullPath);
    var isUsingHelixConvention = xdtFileInfo.Name.StartsWith("Foundation.")
      || xdtFileInfo.Name.StartsWith("Feature.")
      || xdtFileInfo.Name.StartsWith("Project.");

    if(isUsingHelixConvention){
      var targetFileName = String.Join(".",xdtFileInfo.Name.Split('.').Skip(2));
      targetFileName = targetFileName.Substring(0, targetFileName.IndexOf(fileExt) + fileExt.Count());
      targetFilePath = new FilePath(xdtFileInfo.DirectoryName + "/" + targetFileName);
    }

    if(!FileExists(targetFilePath)){
      continue;
    }

    Information($"Transforming config file: {targetFilePath} using transformation file: {xdtFilePath}");
    var transformationLog = XdtTransformConfigWithDefaultLogger(targetFilePath, File(xdtFilePath.FullPath), targetFilePath);
    var hasProblems = (transformationLog.HasError
                        || transformationLog.HasException
                        || transformationLog.HasWarning);

    if(!hasProblems)
    {
        continue;
    }

    Error($"Tranformation log has problems.");
    var problemEntries = transformationLog.Log
                          .Where(x => x.MessageType == "Error"
                                      || x.MessageType == "Exception"
                                      || x.MessageType == "Warning");

    foreach (var entry in problemEntries)
    {
        Error(entry);
    }
  }
}

public class HelixFileNameComparer : IComparer<Cake.Core.IO.FilePath>{
	public int Compare(Cake.Core.IO.FilePath left, Cake.Core.IO.FilePath right){
    var leftFileName = new FileInfo(left.FullPath).Name.ToLower();
    var rightFileName = new FileInfo(right.FullPath).Name.ToLower();
						
		if(leftFileName.StartsWith("foundation")){
			if(rightFileName.StartsWith("foundation"))
				return 0;
			if(rightFileName.StartsWith("feature"))
				return -1;
			if(rightFileName.StartsWith("project"))
				return -1;
			return -1;
		}
		if(leftFileName.StartsWith("feature")){
			if(rightFileName.StartsWith("foundation"))
				return 1;
			if(rightFileName.StartsWith("feature"))
				return 0;
			if(rightFileName.StartsWith("project"))
				return -1;
			return -1;
		}
		if(leftFileName.StartsWith("project")){
			if(rightFileName.StartsWith("foundation"))
				return 1;
			if(rightFileName.StartsWith("feature"))
				return 1;
			if(rightFileName.StartsWith("project"))
				return 0;
			return -1;
		}
		return 0;
	}
}

public void MirrorFiles(string sourcePath, string destinationPath){
  StartProcess("robocopy", new ProcessSettings {
    Arguments = new ProcessArgumentBuilder()
      .Append(@"/MIR /ns /nc /nfl /ndl /np /njs")
      .Append(sourcePath)
      .Append(destinationPath)
    }
  );
}

public void PublishContentSerializedItems(string rootFolder, string layer, string publishRoot)
{
    var rootFolderPath = Directory(rootFolder).Path.FullPath;
    var folders = GetDirectories($"{rootFolder}\\{layer}\\*\\serialization");
    foreach (var folder in folders) {
        var path = folder.FullPath.Replace(rootFolderPath, "");
        var targetDirPath = publishRoot + path;
        Information("Publishing " + folder.FullPath + " to " + targetDirPath);
        CopyDirectory(folder, targetDirPath);
    }
}

public void ApplyXMLTransformsForSitecoreRole(string dirPath, string role, string deploymentTarget){
    var sitecoreRoleWebConfig = dirPath + $"/Web.{role}.config";
    if(FileExists(sitecoreRoleWebConfig)) {
        CopyFile(File(sitecoreRoleWebConfig), File(dirPath + "/Web.config"));        
    }    
    sitecoreRoleWebConfig = dirPath + $"/App_Config/ConnectionStrings.{role}.config";
    if(FileExists(sitecoreRoleWebConfig)) {
        CopyFile(File(sitecoreRoleWebConfig), File(dirPath + "/App_Config/ConnectionStrings.config"));        
    }

    ApplyXmlTransformsForCMS("*.config.common.xdt", dirPath);
    ApplyXmlTransformsForCMS($"*.config.{deploymentTarget}.xdt", dirPath);
    ApplyXmlTransformsForCMS($"*.config.{role}.xdt", dirPath);
    ApplyXmlTransformsForCMS($"*.config.{role}.{deploymentTarget}.xdt", dirPath);    
}