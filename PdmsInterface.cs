// Decompiled with JetBrains decompiler
// Type: IfcModelCollaboration.PdmsInterface
// Assembly: IfcModelCollaboration, Version=2099.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4FD7DE9B-FBF7-4747-A620-F10F7C8F2B9D
// Assembly location: C:\TS_E3D\2.1\TS-E3D_Library\IfcModelCollaboration.dll

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace IfcModelCollaboration
{
  public class PdmsInterface
  {
    public bool UpdateAvevaCreatedObjects = true;
    public bool UpdateAvevaCreatedObjectsHierarchyBasedOnTeklaExportedHierarchy = true;
    public bool CheckAndRecreateAvevaModelDeletedObjects = true;
    public bool KeepDeletedObjectsInDELETED_FRMW = true;
    public bool TeklaDuplicateGUIDsCheckForImportSpecificAvevaSITEsOnly = true;
    public string MappingFolder = string.Empty;
    public bool AvevaSiteCanContainOneTeklaModelOnly;
    public bool UpdateAvevaObjectsEvenTheyHaveNotBeenChangedInTeklaStructures;
    public bool DeleteEmptyContainerElementsAfterImport;
    public const string HierarchiesDotXml = "Hierarchies.xml";
    public const string Stru = "Stru";
    public const string Frmw = "Frmw";
    public const string Name = "Name";
    public const string Negatives = "Negatives";
    public const string DateTimeUtc = "DateTimeUtc";
    public const string Folder = "Folder";
    public const string HierarchyId = "HierarchyId";
    public const string HierarchyName = "HierarchyName";
    public const string Location = "Location";
    public const string ProjecSettingsFile = "ProjectSettings.txt";
    private const string AvevaSiteCanContainOneTeklaModelOnlyParameter = "AvevaSiteCanContainOneTeklaModelOnly";
    private const string UpdateAvevaCreatedObjectsParameter = "UpdateAvevaCreatedObjects";
    private const string UpdateAvevaCreatedObjectsHierarchyBasedOnTeklaExportedHierarchyParameter = "UpdateAvevaCreatedObjectsHierarchyBasedOnTeklaExportedHierarchy";
    private const string UpdateAvevaObjectsEvenTheyHaveNotBeenChangedInTeklaStructuresParameter = "UpdateAvevaObjectsEvenTheyHaveNotBeenChangedInTeklaStructures";
    private const string CheckAndRecreateAvevaModelDeletedObjectsParameter = "CheckAndRecreateAvevaModelDeletedObjects";
    private const string KeepDeletedObjectsInDELETED_FRMWParameter = "KeepDeletedObjectsInDELETED_FRMW";
    private const string DeleteEmptyContainerElementsAfterImportParameter = "DeleteEmptyContainerElementsAfterImport";
    private const string TeklaDuplicateGUIDsCheckForImportSpecificAvevaSITEsOnlyParameter = "TeklaDuplicateGUIDsCheckForImportSpecificAvevaSITEsOnly";
    private const string MappingFolderParameter = "MappingFolder";
    private readonly Tools tools;
    private readonly IfcModelCollaboration.IfcModelCollaboration ifcModelCollaboration;
    private readonly ExportModels exportModels;
    private readonly ImportModels importModels;

    public PdmsInterface()
    {
      if (this.tools == null)
        this.tools = new Tools();
      if (this.ifcModelCollaboration == null)
        this.ifcModelCollaboration = new IfcModelCollaboration.IfcModelCollaboration();
      if (this.exportModels == null)
        this.exportModels = new ExportModels();
      if (this.importModels != null)
        return;
      this.importModels = new ImportModels();
    }

    public ImportToPdmsModel CheckImportVersion(
      string storedVersion,
      ImportToPdmsModel importToPdmsModel)
    {
      ImportToPdmsModel importToPdmsModel1 = importToPdmsModel;
      string newDateTimeUtc = importToPdmsModel.NewDateTimeUtc;
      string dateTimeUtc = importToPdmsModel.DateTimeUtc;
      if (!(storedVersion == dateTimeUtc))
      {
        if (string.IsNullOrEmpty(newDateTimeUtc) && storedVersion != dateTimeUtc)
        {
          importToPdmsModel1.NewDateTimeUtc = dateTimeUtc;
          importToPdmsModel1.DateTimeUtc = storedVersion;
        }
        else if (!string.IsNullOrEmpty(newDateTimeUtc) && storedVersion != dateTimeUtc)
        {
          importToPdmsModel1.NewDateTimeUtc = newDateTimeUtc;
          importToPdmsModel1.DateTimeUtc = storedVersion;
        }
      }
      return importToPdmsModel1;
    }

    public bool GetImportModelInformation(
      string rootFolder,
      ImportToPdmsModel importToPdmsModel,
      out ImportData importData)
    {
      bool flag = true;
      importData = new ImportData();
      importData.DataInformation = new DataInformation();
      importData.Errors = new Errors();
      importData.DataInformation.ExportSoftware = SoftwareOptions.TeklaStructures;
      importData.DataInformation.ImportSoftware = SoftwareOptions.PDMS;
      importData.RootFolder = rootFolder;
      ExportData exportData = new ExportData();
      if (!new FolderHierarchy().HandleFolderHierarchy(rootFolder, IfcModelCollaboration.IfcModelCollaboration.Action.Import, ref importData, ref exportData))
        return false;
      importData.ImportModelFolder = importData.ImportModelsFolder + "\\" + importToPdmsModel.Name;
      importData.NewImportVersionFolder = importData.ImportModelFolder + "\\" + importToPdmsModel.NewDateTimeUtc;
      importData.NewImportVersionDataFolder = importData.NewImportVersionFolder + "\\Data";
      importData.NewImportVersionFile = importData.NewImportVersionDataFolder + "\\" + importToPdmsModel.Name + "#" + importToPdmsModel.NewDateTimeUtc + ".ifc";
      ImportModels importModels = new ImportModels();
      if (!string.IsNullOrEmpty(importToPdmsModel.NewDateTimeUtc))
      {
        string tempFolder = string.Empty;
        if (!File.Exists(importData.NewImportVersionFile))
        {
          string str = importToPdmsModel.Location + "\\" + importToPdmsModel.Name + "#" + importToPdmsModel.NewDateTimeUtc + ".tcZip";
          if (!File.Exists(str))
            str = importToPdmsModel.Location + "\\" + importToPdmsModel.Name + "#" + importToPdmsModel.NewDateTimeUtc + ".ifcZIP";
          if (!importModels.HandleUncompressing(str, ref importData, out tempFolder))
          {
            importData.Errors.ErrorCode = ErrorCode.UncompressingFailed;
            importData.Errors.ErrorInfo = str;
            return false;
          }
          string importVersionFolder = importData.NewImportVersionFolder;
          if (!importModels.HandlePacketContent(tempFolder, importVersionFolder, ref importData))
          {
            importData.Errors.ErrorCode = ErrorCode.FileCopyFailed;
            importData.Errors.ErrorInfo = importVersionFolder;
            return false;
          }
        }
      }
      else
      {
        flag = false;
        importData.PreviousImportVersionFolder = importData.ImportModelFolder + "\\" + importToPdmsModel.DateTimeUtc;
        importData.PreviousImportVersionFile = importData.PreviousImportVersionFolder + "\\" + importToPdmsModel.Name + "#" + importToPdmsModel.DateTimeUtc + ".ifc";
        importData.PreviousImportVersionDataFolder = importData.PreviousImportVersionFolder + "\\Data";
        importData.NewImportVersionDataFolder = importData.PreviousImportVersionDataFolder;
        importData.NewImportVersionFile = importData.PreviousImportVersionFile;
        if (!File.Exists(importData.PreviousImportVersionDataFolder + "\\" + importToPdmsModel.Name + "#" + importToPdmsModel.DateTimeUtc + ".ifc"))
          return false;
      }
      if (!new DataInformationHandler().ReadDataInformation(importData.NewImportVersionDataFolder, ref importData))
      {
        if (importData.Errors.ErrorCode != ErrorCode.PacketIsNotValid)
          importData.Errors.ErrorCode = ErrorCode.DataInformationReadingFailed;
        importData.NewImportVersionFile = string.Empty;
        return false;
      }
      if (!this.ReadPdmsModelInformation(ref importData))
        return false;
      if (!flag)
      {
        importData.NewImportVersionDataFolder = string.Empty;
        importData.NewImportVersionFile = string.Empty;
      }
      return true;
    }

    public bool GetImportModelsInformation(
      string rootFolder,
      out List<ImportToPdmsModel> list,
      DateTime modelOpeningTimeUTC,
      out ImportData importData)
    {
      list = (List<ImportToPdmsModel>) null;
      importData = new ImportData();
      importData.DataInformation = new DataInformation();
      importData.Errors = new Errors();
      importData.DataInformation.ExportSoftware = SoftwareOptions.TeklaStructures;
      importData.DataInformation.ImportSoftware = SoftwareOptions.PDMS;
      importData.RootFolder = rootFolder;
      ExportData exportData = new ExportData();
      return new FolderHierarchy().HandleFolderHierarchy(rootFolder, IfcModelCollaboration.IfcModelCollaboration.Action.Import, ref importData, ref exportData) && this.ReadPdmsVersionsAndTheirSettings(ref importData, out list, modelOpeningTimeUTC);
    }

    public bool StoreImportModelToPdmsData(
      string rootFolder,
      ImportToPdmsModel importVersionData,
      ref ImportData importData)
    {
      if (importData == null)
        importData = new ImportData();
      if (importData.DataInformation == null)
        importData.DataInformation = new DataInformation();
      importData.DataInformation.ImportSoftware = SoftwareOptions.PDMS;
      importData.DataInformation.ExportSoftware = SoftwareOptions.TeklaStructures;
      ExportData exportData = new ExportData();
      return new FolderHierarchy().HandleFolderHierarchy(rootFolder, IfcModelCollaboration.IfcModelCollaboration.Action.Import, ref importData, ref exportData) && this.StoreImportToPdmsModelInformation(importVersionData, ref importData);
    }

    public bool FirstImportToPdmsData(
      string file,
      string rootFolder,
      out ImportData importData,
      out ImportToPdmsModel importToPdmsModel)
    {
      importData = new ImportData();
      importData.DataInformation = new DataInformation();
      importData.Errors = new Errors();
      importData.DataInformation.ExportSoftware = SoftwareOptions.TeklaStructures;
      importData.DataInformation.ImportSoftware = SoftwareOptions.PDMS;
      importData.RootFolder = rootFolder;
      importToPdmsModel = new ImportToPdmsModel();
      ExportData exportData = new ExportData();
      string nameReturn;
      string dateTimeUtc;
      string folder;
      if (!File.Exists(file) || !new FolderHierarchy().HandleFolderHierarchy(importData.RootFolder, IfcModelCollaboration.IfcModelCollaboration.Action.Import, ref importData, ref exportData) || !new ImportModels().HandleModel(file, ref importData, out nameReturn, out dateTimeUtc, out folder))
        return false;
      importToPdmsModel.NewDateTimeUtc = dateTimeUtc;
      importToPdmsModel.Name = nameReturn;
      importToPdmsModel.Location = folder;
      return this.ReadPdmsModelInformation(ref importData);
    }

    public bool UpdateImportToPdmsData(
      ImportToPdmsModel importToPdmsModelUpdate,
      string rootFolder,
      string attributeMappingFile,
      out ImportData importData,
      out DataSet dataSet,
      bool hierarchy)
    {
      dataSet = new DataSet();
      importData = new ImportData();
      importData.DataInformation = new DataInformation();
      importData.Errors = new Errors();
      importData.DataInformation.ExportSoftware = SoftwareOptions.TeklaStructures;
      importData.DataInformation.ImportSoftware = SoftwareOptions.PDMS;
      importData.RootFolder = rootFolder;
      ExportData exportData = new ExportData();
      string location = importToPdmsModelUpdate.Location;
      if (!Directory.Exists(location) || !new FolderHierarchy().HandleFolderHierarchy(importData.RootFolder, IfcModelCollaboration.IfcModelCollaboration.Action.Import, ref importData, ref exportData))
        return false;
      string name = importToPdmsModelUpdate.Name;
      string newDateTimeUtc = importToPdmsModelUpdate.NewDateTimeUtc;
      string str = location + "\\" + name + "#" + newDateTimeUtc + ".tcZip";
      if (!File.Exists(str))
        str = location + "\\" + name + "#" + newDateTimeUtc + ".ifcZIP";
      if (!File.Exists(str))
      {
        importData.Errors.ErrorCode = ErrorCode.FileDoesNotExist;
        importData.Errors.ErrorInfo = str;
        return false;
      }
      string nameReturn;
      string dateTimeUtc;
      string folder;
      if (!new ImportModels().HandleModel(str, ref importData, out nameReturn, out dateTimeUtc, out folder))
        return false;
      string path1 = importData.ImportModelFolder + "\\" + importToPdmsModelUpdate.DateTimeUtc;
      if (Directory.Exists(path1))
        importData.PreviousImportVersionFolder = path1;
      string path2 = path1 + "\\Data\\" + importToPdmsModelUpdate.Name + "#" + importToPdmsModelUpdate.DateTimeUtc + ".ifc";
      if (File.Exists(path2))
      {
        importData.PreviousImportVersionFile = path2;
        List<string> attributeFilterList = new List<string>();
        string cacheFolder = importData.ImportModelFolder + "\\Caches";
        if (!string.IsNullOrEmpty(importData.NewImportVersionFile) && !string.IsNullOrEmpty(importData.PreviousImportVersionFile) && !this.CompareModels(importData.PreviousImportVersionFile, importData.NewImportVersionFile, attributeFilterList, cacheFolder, attributeMappingFile, ref importData, out dataSet, hierarchy) || !this.ReadPdmsModelInformation(ref importData))
          return false;
        this.tools.CleanOldVersions(importData, importToPdmsModelUpdate.Name, importToPdmsModelUpdate.NewDateTimeUtc, importToPdmsModelUpdate.DateTimeUtc);
        return true;
      }
      importData.Errors.ErrorCode = ErrorCode.FileDoesNotExist;
      importData.Errors.ErrorInfo = path2;
      return false;
    }

    public bool RemoveBackUpFiles(string rootFolder, out Errors errors)
    {
      errors = new Errors();
      errors.ErrorCode = ErrorCode.None;
      ExportData exportData = new ExportData();
      ImportData importData = new ImportData()
      {
        DataInformation = new DataInformation(),
        Errors = new Errors()
      };
      importData.DataInformation.ExportSoftware = SoftwareOptions.TeklaStructures;
      importData.DataInformation.ImportSoftware = SoftwareOptions.PDMS;
      importData.RootFolder = rootFolder;
      if (!new FolderHierarchy().HandleFolderHierarchy(rootFolder, IfcModelCollaboration.IfcModelCollaboration.Action.Import, ref importData, ref exportData))
      {
        errors = importData.Errors;
        return false;
      }
      foreach (DirectoryInfo directory in new DirectoryInfo(importData.ImportModelsFolder).GetDirectories())
      {
        if (!(directory.Name == "Settings"))
        {
          string path1 = importData.ImportModelsFolder + "\\Settings";
          string str = importData.ImportModelsFolder + "\\" + directory.Name + "\\Settings\\Settings.xml";
          if (!Directory.Exists(path1))
            return true;
          string path2 = str + ".bak0";
          string path3 = str + ".bak";
          if (File.Exists(path2))
          {
            try
            {
              File.Delete(path2);
            }
            catch (Exception ex)
            {
              errors.ErrorCode = ErrorCode.DeletingOfFileFailed;
              Errors errors1 = errors;
              errors1.ErrorInfo = errors1.ErrorInfo + path2 + Environment.NewLine;
              return false;
            }
          }
          if (File.Exists(path3))
          {
            try
            {
              File.Delete(path3);
            }
            catch
            {
              errors.ErrorCode = ErrorCode.DeletingOfFileFailed;
              Errors errors1 = errors;
              errors1.ErrorInfo = errors1.ErrorInfo + path3 + Environment.NewLine;
              return false;
            }
          }
        }
      }
      return true;
    }

    public bool GetModelAttributeDefinitions(
      ImportToPdmsModel importToPdmsModel,
      string rootFolder,
      out ImportData importData,
      out Dictionary<string, string> attributeDefinitions)
    {
      attributeDefinitions = new Dictionary<string, string>();
      DataTable dataTable = (DataTable) null;
      importData = new ImportData();
      importData.DataInformation = new DataInformation();
      importData.Errors = new Errors();
      importData.DataInformation.ExportSoftware = SoftwareOptions.TeklaStructures;
      importData.DataInformation.ImportSoftware = SoftwareOptions.PDMS;
      importData.RootFolder = rootFolder;
      ExportData exportData = new ExportData();
      string location = importToPdmsModel.Location;
      if (!Directory.Exists(location) || !new FolderHierarchy().HandleFolderHierarchy(importData.RootFolder, IfcModelCollaboration.IfcModelCollaboration.Action.Import, ref importData, ref exportData))
        return false;
      importData.ImportModelFolder = importData.ImportModelsFolder + "\\" + importToPdmsModel.Name;
      importData.NewImportVersionFolder = importData.ImportModelFolder + "\\" + importToPdmsModel.NewDateTimeUtc;
      importData.NewImportVersionDataFolder = importData.NewImportVersionFolder + "\\Data";
      importData.NewImportVersionFile = importData.NewImportVersionDataFolder + "\\" + importToPdmsModel.Name + "#" + importToPdmsModel.NewDateTimeUtc + ".ifc";
      string name = importToPdmsModel.Name;
      string newDateTimeUtc = importToPdmsModel.NewDateTimeUtc;
      if (!File.Exists(location + "\\" + name + "#" + newDateTimeUtc + ".ifcZIP"))
        return false;
      string cacheFolder = importData.ImportModelFolder + "\\Caches";
      if (!string.IsNullOrEmpty(importData.NewImportVersionFile) && !this.GetAttributeDefinitions(importData.NewImportVersionFile, cacheFolder, ref importData, out dataTable))
      {
        importData.Errors.ErrorCode = ErrorCode.DataMissing;
        importData.Errors.ErrorInfo = "Reading of attribute definitions failed";
        return false;
      }
      if (dataTable != null)
      {
        foreach (DataRow row in (InternalDataCollectionBase) dataTable.Rows)
        {
          string key = row["Key"].ToString();
          string str = row["Value"].ToString();
          if (!attributeDefinitions.ContainsKey(key))
            attributeDefinitions.Add(key, str);
        }
      }
      return true;
    }

    public bool RemoveImportInstance(string rootFolder, string modelName)
    {
      return new IfcModelCollaboration.IfcModelCollaboration().RemoveImportInstance(rootFolder, SoftwareOptions.TeklaStructures, modelName);
    }

    public bool GetMappedProfileInfo(
      string profileMappingFile,
      List<string> profiles,
      ref Dictionary<string, string> libraryProfiles,
      ref Dictionary<string, Tuple<string, List<string>>> parametricProfiles,
      out Errors errors,
      out Dictionary<string, string> profileMapping,
      out DataTable profileMappingTable,
      bool isImp = false)
    {
      errors = new Errors();
      profileMappingTable = (DataTable) null;
      return new Profiles().GetMappedInfo(isImp, profileMappingFile, profiles, ref libraryProfiles, ref parametricProfiles, out errors, out profileMapping, out profileMappingTable);
    }

    public bool GetProfileMapping(string profileMappingFile, out Dictionary<string, string> mapped)
    {
      return new Profiles().GetProfileMapping(profileMappingFile, out mapped);
    }

    public bool SaveProfileMapping(
      string profileMappingFile,
      Dictionary<string, string> profileMapping,
      out Errors errors)
    {
      errors = new Errors();
      return new Profiles().SaveProfileMapping(profileMappingFile, profileMapping, out errors);
    }

    public bool GetMaterialMapping(
      string materialMappingFile,
      ref Dictionary<string, string> materialMapping,
      out Errors errors)
    {
      errors = new Errors();
      return new Materials().GetMaterialMapping(materialMappingFile, ref materialMapping, out errors);
    }

    public bool SaveMaterialMapping(
      string materialMappingFile,
      Dictionary<string, string> materialMapping,
      string header,
      out Errors errors)
    {
      errors = new Errors();
      return new Materials().SaveMaterialMapping(materialMappingFile, materialMapping, header, out errors);
    }

    public bool GetAttributeMapping(
      string attributeMappingFile,
      out DataTable attributeMapping,
      out Errors errors)
    {
      errors = new Errors();
      return new Attributes().GetAttributeMapping(attributeMappingFile, out attributeMapping, out errors);
    }

    public bool SaveAttributeMapping(
      string attributeMappingFile,
      DataTable attributeMapping,
      out Errors errors)
    {
      errors = new Errors();
      return new Attributes().SaveAttributeMapping(attributeMappingFile, attributeMapping, out errors);
    }

    public bool GetAttributeMappingPDMS(
      string attributeMappingFile,
      out DataTable attributeMapping,
      out Errors errors)
    {
      errors = new Errors();
      return new Attributes().GetAttributeMappingPDMS(attributeMappingFile, out attributeMapping, out errors);
    }

    public bool SaveAttributeMappingPDMS(
      string attributeMappingFile,
      DataTable attributeMapping,
      out Errors errors)
    {
      errors = new Errors();
      return new Attributes().SaveAttributeMappingPDMS(attributeMappingFile, attributeMapping, out errors);
    }

    public bool GetHierarchies(string folder, out DataTable dataTable)
    {
      DataSet dataSet = new DataSet();
      if (!new XmlHandler().ReadXmlToDataSet(Path.Combine(folder, "Hierarchies.xml"), out dataSet))
      {
        dataTable = (DataTable) null;
        return false;
      }
      dataTable = dataSet.Tables[0];
      return true;
    }

    public ProjectSettings GetProjectSettings(string rootFolder, out Errors errors)
    {
      errors = new Errors();
      Dictionary<string, object> dictionary = new Dictionary<string, object>();
      ImportData importData = new ImportData();
      importData.DataInformation = new DataInformation()
      {
        ExportSoftware = SoftwareOptions.TeklaStructures
      };
      ExportData exportData = new ExportData();
      if (!new FolderHierarchy().HandleFolderHierarchy(rootFolder, IfcModelCollaboration.IfcModelCollaboration.Action.Import, ref importData, ref exportData))
      {
        errors.ErrorCode = ErrorCode.FolderCreationFailed;
        errors.ErrorInfo = "Settings folder creation failed";
      }
      string path = Path.Combine(importData.ImportModelsSettingsFolder, "ProjectSettings.txt");
      ProjectSettings projectSettings = new ProjectSettings();
      if (File.Exists(path))
      {
        try
        {
          using (StreamReader streamReader = new StreamReader(path))
          {
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
              if (!line.StartsWith("//"))
              {
                if (line.StartsWith("UpdateAvevaCreatedObjectsHierarchyBasedOnTeklaExportedHierarchy"))
                  this.HandleSetting(ref projectSettings, line, "UpdateAvevaCreatedObjectsHierarchyBasedOnTeklaExportedHierarchy", ref errors);
                else if (line.StartsWith("UpdateAvevaCreatedObjects"))
                  this.HandleSetting(ref projectSettings, line, "UpdateAvevaCreatedObjects", ref errors);
                else if (line.StartsWith("MappingFolder"))
                  this.HandleSetting(ref projectSettings, line, "MappingFolder", ref errors);
                else if (line.StartsWith("CheckAndRecreateAvevaModelDeletedObjects"))
                  this.HandleSetting(ref projectSettings, line, "CheckAndRecreateAvevaModelDeletedObjects", ref errors);
                else if (line.StartsWith("UpdateAvevaObjectsEvenTheyHaveNotBeenChangedInTeklaStructures"))
                  this.HandleSetting(ref projectSettings, line, "UpdateAvevaObjectsEvenTheyHaveNotBeenChangedInTeklaStructures", ref errors);
                else if (line.StartsWith("KeepDeletedObjectsInDELETED_FRMW"))
                  this.HandleSetting(ref projectSettings, line, "KeepDeletedObjectsInDELETED_FRMW", ref errors);
                else if (line.StartsWith("DeleteEmptyContainerElementsAfterImport"))
                  this.HandleSetting(ref projectSettings, line, "DeleteEmptyContainerElementsAfterImport", ref errors);
                else if (line.StartsWith("AvevaSiteCanContainOneTeklaModelOnly"))
                  this.HandleSetting(ref projectSettings, line, "AvevaSiteCanContainOneTeklaModelOnly", ref errors);
                else if (line.StartsWith("TeklaDuplicateGUIDsCheckForImportSpecificAvevaSITEsOnly"))
                  this.HandleSetting(ref projectSettings, line, "TeklaDuplicateGUIDsCheckForImportSpecificAvevaSITEsOnly", ref errors);
              }
            }
          }
        }
        catch (Exception ex)
        {
          errors.ErrorCode = ErrorCode.ReadingOfFileFailed;
          errors.ErrorInfo = "Settings file " + path;
        }
      }
      else
      {
        try
        {
          using (StreamWriter streamWriter = new StreamWriter(path))
          {
            streamWriter.WriteLine("UpdateAvevaCreatedObjects=" + projectSettings.UpdateAvevaCreatedObjects.ToString());
            streamWriter.WriteLine("UpdateAvevaCreatedObjectsHierarchyBasedOnTeklaExportedHierarchy=" + projectSettings.UpdateAvevaCreatedObjectsHierarchyBasedOnTeklaExportedHierarchy.ToString());
            streamWriter.WriteLine("CheckAndRecreateAvevaModelDeletedObjects=" + projectSettings.CheckAndRecreateAvevaModelDeletedObjects.ToString());
            streamWriter.WriteLine("UpdateAvevaObjectsEvenTheyHaveNotBeenChangedInTeklaStructures=" + projectSettings.UpdateAvevaObjectsEvenTheyHaveNotBeenChangedInTeklaStructures.ToString());
            streamWriter.WriteLine("KeepDeletedObjectsInDELETED_FRMW=" + projectSettings.KeepDeletedObjectsInDELETED_FRMW.ToString());
            streamWriter.WriteLine("DeleteEmptyContainerElementsAfterImport=" + projectSettings.DeleteEmptyContainerElementsAfterImport.ToString());
            streamWriter.WriteLine("AvevaSiteCanContainOneTeklaModelOnly=" + projectSettings.AvevaSiteCanContainOneTeklaModelOnly.ToString());
            streamWriter.WriteLine("TeklaDuplicateGUIDsCheckForImportSpecificAvevaSITEsOnly=" + projectSettings.TeklaDuplicateGUIDsCheckForImportSpecificAvevaSITEsOnly.ToString());
          }
        }
        catch (Exception ex)
        {
          errors.ErrorCode = ErrorCode.FileCreationFailed;
          errors.ErrorInfo = "Settings file " + path;
        }
      }
      return projectSettings;
    }

    private void HandleSetting(
      ref ProjectSettings projectSettings,
      string line,
      string setting,
      ref Errors errors)
    {
      bool flag = false;
      string empty = string.Empty;
      string[] strArray = line.Split('=');
      if (strArray.Length == 2 && strArray[1].ToUpper() == "TRUE")
        flag = true;
      else if (strArray.Length == 2)
      {
        empty = strArray[1];
      }
      else
      {
        Errors errors1 = errors;
        errors1.ErrorInfo = errors1.ErrorInfo + line + " issues in reading,";
      }
      // ISSUE: reference to a compiler-generated method
      switch (\u003CPrivateImplementationDetails\u003E.ComputeStringHash(setting))
      {
        case 3633823:
          if (!(setting == "UpdateAvevaCreatedObjects"))
            break;
          projectSettings.UpdateAvevaCreatedObjects = flag;
          break;
        case 887737954:
          if (!(setting == "DeleteEmptyContainerElementsAfterImport"))
            break;
          projectSettings.DeleteEmptyContainerElementsAfterImport = flag;
          break;
        case 1533402890:
          if (!(setting == "KeepDeletedObjectsInDELETED_FRMW"))
            break;
          projectSettings.KeepDeletedObjectsInDELETED_FRMW = flag;
          break;
        case 1719725597:
          if (!(setting == "MappingFolder"))
            break;
          projectSettings.MappingFolder = empty;
          break;
        case 1903001723:
          if (!(setting == "UpdateAvevaCreatedObjectsHierarchyBasedOnTeklaExportedHierarchy"))
            break;
          projectSettings.UpdateAvevaCreatedObjectsHierarchyBasedOnTeklaExportedHierarchy = flag;
          break;
        case 2306667900:
          if (!(setting == "TeklaDuplicateGUIDsCheckForImportSpecificAvevaSITEsOnly"))
            break;
          projectSettings.TeklaDuplicateGUIDsCheckForImportSpecificAvevaSITEsOnly = flag;
          break;
        case 2593365336:
          if (!(setting == "UpdateAvevaObjectsEvenTheyHaveNotBeenChangedInTeklaStructures"))
            break;
          projectSettings.UpdateAvevaObjectsEvenTheyHaveNotBeenChangedInTeklaStructures = flag;
          break;
        case 2720185374:
          if (!(setting == "CheckAndRecreateAvevaModelDeletedObjects"))
            break;
          projectSettings.CheckAndRecreateAvevaModelDeletedObjects = flag;
          break;
        case 3628609909:
          if (!(setting == "AvevaSiteCanContainOneTeklaModelOnly"))
            break;
          projectSettings.AvevaSiteCanContainOneTeklaModelOnly = flag;
          break;
      }
    }

    public void GetExportSettings(
      string rootFolder,
      out List<ExportToTeklaModel> listOfModels,
      out ExportData data)
    {
      listOfModels = this.GetIfcExportInformation(SoftwareOptions.PDMS, SoftwareOptions.TeklaStructures, rootFolder, out data);
    }

    public List<ExportToTeklaModel> GetIfcExportInformation(
      SoftwareOptions exportSoftware,
      SoftwareOptions importSoftware,
      string rootFolder,
      out ExportData data)
    {
      data = new ExportData();
      data.DataInformation = new DataInformation();
      data.Errors = new Errors();
      data.DataInformation.ExportSoftware = exportSoftware;
      data.DataInformation.ImportSoftware = importSoftware;
      data.RootFolder = rootFolder;
      ImportData importData = new ImportData();
      if (!new FolderHierarchy().HandleFolderHierarchy(rootFolder, IfcModelCollaboration.IfcModelCollaboration.Action.Export, ref importData, ref data))
        return (List<ExportToTeklaModel>) null;
      if (string.IsNullOrEmpty(data.ExportModelsFolder))
        return (List<ExportToTeklaModel>) null;
      List<object> list = new List<object>();
      if (!this.exportModels.GetExportModelsData((object) new ExportToTeklaModel(), ref data, out list))
        return (List<ExportToTeklaModel>) null;
      return list.Cast<ExportToTeklaModel>().ToList<ExportToTeklaModel>();
    }

    public bool ExportToTeklaFolders(
      SoftwareOptions exportSoftware,
      SoftwareOptions importSoftware,
      string rootFolder,
      ExportVersionData exportVersionData,
      out ExportData data)
    {
      data = new ExportData();
      data.DataInformation = new DataInformation();
      data.Errors = new Errors();
      data.DataInformation.ExportSoftware = exportSoftware;
      data.DataInformation.ImportSoftware = importSoftware;
      data.RootFolder = rootFolder;
      ImportData importData = new ImportData();
      if (!new FolderHierarchy().HandleFolderHierarchy(rootFolder, IfcModelCollaboration.IfcModelCollaboration.Action.Export, ref importData, ref data) || string.IsNullOrEmpty(data.ExportModelsFolder))
        return false;
      if (string.IsNullOrEmpty(exportVersionData.Folder))
        exportVersionData.Folder = rootFolder + "\\ExportTo" + importSoftware.ToString();
      if (exportVersionData.Folder.StartsWith("."))
        exportVersionData.FolderAbsolute = Path.Combine(data.RootFolder, exportVersionData.Folder);
      if (exportVersionData.Folder.Contains(rootFolder))
      {
        exportVersionData.FolderAbsolute = exportVersionData.Folder;
        exportVersionData.Folder = exportVersionData.Folder.Replace(rootFolder, ".\\");
      }
      return true;
    }

    public bool ExportModelToTekla(
      string rootFolder,
      ref ExportToTeklaModel exportVersionData,
      out ErrorCode errorCode,
      out List<string> errorList,
      out string tobezipped,
      out ExportData exportData,
      bool isImperial = false)
    {
      errorList = new List<string>();
      tobezipped = string.Empty;
      errorCode = ErrorCode.None;
      exportData = new ExportData();
      bool flag = true;
      if (!this.ValidateExportVersionData(exportVersionData))
      {
        errorCode = ErrorCode.DataMissing;
        return false;
      }
      if (!this.ifcModelCollaboration.CreateTempExportFolder(out tobezipped))
      {
        errorCode = ErrorCode.FolderCreationFailed;
        return false;
      }
      string name = exportVersionData.Name;
      if (!this.WriteDataInformation(tobezipped, ref exportVersionData, isImperial))
      {
        errorCode = ErrorCode.DataInformationCreationFailed;
        return false;
      }
      if (this.ExportToTeklaFolders(SoftwareOptions.PDMS, SoftwareOptions.TeklaStructures, rootFolder, exportVersionData, out exportData))
        return flag;
      errorCode = ErrorCode.DataInformationCreationFailed;
      return false;
    }

    public bool ExportModelToTeklaNext(
      ref ExportToTeklaModel exportVersionData,
      string tobezipped,
      ref ExportData exportData)
    {
      if (exportData.Errors == null)
        exportData.Errors = new Errors();
      string extension = ".tcZip";
      if (!this.ifcModelCollaboration.ZipFilesToTekla(tobezipped, exportVersionData.FolderAbsolute, exportVersionData.NameFull, extension))
      {
        exportData.Errors.ErrorCode = ErrorCode.CompressingFailed;
        exportData.Errors.ErrorInfo = tobezipped;
        return false;
      }
      try
      {
        Directory.Delete(tobezipped, true);
      }
      catch (Exception ex)
      {
        exportData.Errors.ErrorCode = ErrorCode.CompressingFailed;
        exportData.Errors.ErrorInfo = tobezipped;
        return false;
      }
      return this.StoreExportToTeklaModelInformation(exportVersionData, ref exportData);
    }

    public bool ExportModelToTeklaStoreSettings(
      string rootFolder,
      ref ExportToTeklaModel exportVersionData,
      out ExportData exportData,
      out ErrorCode errorCode)
    {
      errorCode = ErrorCode.None;
      exportData = new ExportData();
      if (!this.ExportToTeklaFolders(SoftwareOptions.PDMS, SoftwareOptions.TeklaStructures, rootFolder, exportVersionData, out exportData))
      {
        errorCode = ErrorCode.DataInformationCreationFailed;
        return false;
      }
      List<ExportToTeklaModel> listOfModels;
      this.GetExportSettings(rootFolder, out listOfModels, out exportData);
      foreach (ExportToTeklaModel exportToTeklaModel in listOfModels)
      {
        if (exportToTeklaModel.Name == exportVersionData.Name)
        {
          errorCode = ErrorCode.NameReceived;
          return false;
        }
      }
      if (this.StoreExportToTeklaModelInformation(exportVersionData, ref exportData))
        return true;
      errorCode = ErrorCode.ExportSettingsStoringFailed;
      return false;
    }

    public bool RemoveExportInstance(string rootFolder, string selectedModel)
    {
      return this.ifcModelCollaboration.RemoveExportInstance(rootFolder, SoftwareOptions.TeklaStructures, selectedModel);
    }

    internal bool ReadPdmsModelInformation(ref ImportData importData)
    {
      if (!new ImportModels().CreateProfileList(ref importData))
      {
        importData.Errors.ErrorCode = ErrorCode.ReadingOfDataFailed;
        importData.Errors.ErrorInfo = "profiles";
        return false;
      }
      Materials materials = new Materials();
      importData.NewImportVersionMaterials = materials.GetMaterialsDictionary(importData.NewImportVersionDataFolder, ref importData);
      if (importData.Errors.ErrorCode == ErrorCode.None)
        return true;
      importData.Errors.ErrorCode = ErrorCode.ReadingOfDataFailed;
      importData.Errors.ErrorInfo = "materials";
      return false;
    }

    public bool CompareModels(
      string olderFile,
      string newerFile,
      List<string> attributeFilterList,
      string cacheFolder,
      string attributeMappingFile,
      ref ImportData importData,
      out DataSet dataSet,
      bool hierarchy)
    {
      dataSet = new DataSet();
      string empty = string.Empty;
      attributeFilterList = this.ReadAttributesFilter(attributeMappingFile, attributeFilterList);
      string str1 = this.WriteAttributeFilter(hierarchy, attributeFilterList);
      if (!File.Exists(olderFile) || !File.Exists(newerFile))
      {
        importData.Errors.ErrorCode = ErrorCode.FileDoesNotExist;
        if (!File.Exists(olderFile))
          importData.Errors.ErrorInfo = olderFile;
        if (!File.Exists(olderFile))
          importData.Errors.ErrorInfo = newerFile;
        return false;
      }
      FileInfo fileInfo1 = new FileInfo(olderFile);
      FileInfo fileInfo2 = new FileInfo(newerFile);
      try
      {
        if (!Directory.Exists(cacheFolder))
          Directory.CreateDirectory(cacheFolder);
      }
      catch (Exception ex)
      {
        importData.Errors.ErrorCode = ErrorCode.FolderCreationFailed;
        importData.Errors.ErrorInfo = cacheFolder;
        return false;
      }
      string str2 = cacheFolder + "\\" + fileInfo2.Name.Remove(0, fileInfo2.Name.IndexOf('#')) + "-" + fileInfo1.Name.Remove(0, fileInfo2.Name.IndexOf('#')) + ".xml";
      string str3 = (str2 + ";" + olderFile + ";" + newerFile + ";" + cacheFolder + ";" + str1).Replace(" ", "{replace}");
      FileInfo fileInfo3 = new FileInfo(Assembly.GetExecutingAssembly().Location);
      if (fileInfo3.Directory != null && fileInfo3.Directory.Parent != null)
      {
        string path = fileInfo3.Directory.Parent.FullName + "\\CompareIfcModels\\CompareIfcModels.exe";
        Process process = new Process();
        if (File.Exists(path))
        {
          process.StartInfo.FileName = path;
          process.StartInfo.Arguments = str3;
          try
          {
            process.Start();
            process.WaitForExit();
          }
          catch
          {
            importData.Errors.ErrorCode = ErrorCode.ReadingOfDataFailed;
            importData.Errors.ErrorInfo = "Launching of " + path + "failed. Args: " + str3;
            return false;
          }
        }
        else
        {
          importData.Errors.ErrorCode = ErrorCode.FileDoesNotExist;
          importData.Errors.ErrorInfo = path;
          return false;
        }
      }
      if (File.Exists(str2))
      {
        Tools tools = new Tools();
        dataSet = tools.ReadXmlToDataSet(str2, ref importData);
        return true;
      }
      importData.Errors.ErrorCode = ErrorCode.DataMissing;
      importData.Errors.ErrorInfo = this.ReadLogFile(cacheFolder) + " xml: " + str2;
      return false;
    }

    private string WriteAttributeFilter(bool hierarchy, List<string> attributeFilterList)
    {
      string str = string.Empty;
      if (hierarchy)
      {
        if (!attributeFilterList.Contains("Hierarchy.AvevaSITE"))
          attributeFilterList.Add("Hierarchy.AvevaSITE");
        if (!attributeFilterList.Contains("Hierarchy.AvevaZONE"))
          attributeFilterList.Add("Hierarchy.AvevaZONE");
        if (!attributeFilterList.Contains("Hierarchy.AvevaSTRU"))
          attributeFilterList.Add("Hierarchy.AvevaSTRU");
        if (!attributeFilterList.Contains("Hierarchy.AvevaFRMW"))
          attributeFilterList.Add("Hierarchy.AvevaFRMW");
        if (!attributeFilterList.Contains("Hierarchy.AvevaSBFR"))
          attributeFilterList.Add("Hierarchy.AvevaSBFR");
      }
      foreach (string attributeFilter in attributeFilterList)
        str = !string.IsNullOrEmpty(str) ? str + ":" + attributeFilter : attributeFilter;
      return str;
    }

    private List<string> ReadAttributesFilter(string attriFilterFile, List<string> attriFilter)
    {
      attriFilter = new List<string>();
      try
      {
        using (StreamReader streamReader = new StreamReader(attriFilterFile))
        {
          string str1;
          while ((str1 = streamReader.ReadLine()) != null)
          {
            str1.TrimStart();
            if (!str1.StartsWith("/"))
            {
              string[] strArray = str1.Split(';');
              if (strArray.Length == 4)
              {
                string str2 = strArray[0];
                str2.TrimEnd();
                try
                {
                  if (!str2.Contains("."))
                    str2 = "." + str2;
                  if (!attriFilter.Contains(str2))
                    attriFilter.Add(str2);
                }
                catch (Exception ex)
                {
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
      }
      return attriFilter;
    }

    private static List<string> GetAttributesFilter(string attriFilterFile)
    {
      List<string> stringList = new List<string>();
      foreach (DataTable table in (InternalDataCollectionBase) PdmsInterface.ReadXmlToDataSet(attriFilterFile).Tables)
      {
        foreach (object row in (InternalDataCollectionBase) table.Rows)
        {
          DataRow dataRow = row as DataRow;
          try
          {
            if (dataRow != null)
            {
              string str = dataRow["ifcName"].ToString();
              if (!string.IsNullOrEmpty(str))
              {
                if (!str.Contains("."))
                  str = "." + str;
                if (!stringList.Contains(str))
                  stringList.Add(str);
              }
            }
          }
          catch (Exception ex)
          {
          }
        }
      }
      return stringList;
    }

    private static DataSet ReadXmlToDataSet(string file)
    {
      DataSet dataSet = new DataSet();
      if (File.Exists(file))
      {
        try
        {
          int num = (int) dataSet.ReadXml(file);
        }
        catch
        {
        }
      }
      return dataSet;
    }

    private bool GetAttributeDefinitions(
      string modelFile,
      string cacheFolder,
      ref ImportData importData,
      out DataTable dataTable)
    {
      dataTable = new DataTable();
      if (!File.Exists(modelFile))
      {
        importData.Errors.ErrorCode = ErrorCode.FileDoesNotExist;
        if (!File.Exists(modelFile))
          importData.Errors.ErrorInfo = modelFile;
        return false;
      }
      FileInfo fileInfo1 = new FileInfo(modelFile);
      try
      {
        if (!Directory.Exists(cacheFolder))
          Directory.CreateDirectory(cacheFolder);
      }
      catch (Exception ex)
      {
        importData.Errors.ErrorCode = ErrorCode.FolderCreationFailed;
        importData.Errors.ErrorInfo = cacheFolder;
        return false;
      }
      string str1 = cacheFolder + "\\Attr-" + fileInfo1.Name.Replace(fileInfo1.Extension, string.Empty) + "-" + fileInfo1.Name.Replace(fileInfo1.Extension, string.Empty) + ".xml";
      string str2 = (str1 + ";" + modelFile + ";" + cacheFolder).Replace(" ", "{replace}");
      FileInfo fileInfo2 = new FileInfo(Assembly.GetExecutingAssembly().Location);
      if (fileInfo2.Directory != null && fileInfo2.Directory.Parent != null)
      {
        string path = fileInfo2.Directory.Parent.FullName + "\\CompareIfcModels\\AttributeDefinitions.exe";
        Process process = new Process();
        if (File.Exists(path))
        {
          process.StartInfo.FileName = path;
          process.StartInfo.Arguments = str2;
          try
          {
            process.Start();
            process.WaitForExit();
          }
          catch
          {
            importData.Errors.ErrorCode = ErrorCode.ReadingOfDataFailed;
            importData.Errors.ErrorInfo = "Launching of " + path + "failed";
            return false;
          }
        }
        else
        {
          importData.Errors.ErrorCode = ErrorCode.FileDoesNotExist;
          importData.Errors.ErrorInfo = path;
          return false;
        }
      }
      if (File.Exists(str1))
      {
        new XmlHandler().ReadXmlToDataTable(str1, "AttributeDefinitions", out dataTable);
        return true;
      }
      importData.Errors.ErrorCode = ErrorCode.DataMissing;
      importData.Errors.ErrorInfo = this.ReadLogFile(cacheFolder);
      return false;
    }

    private string ReadLogFile(string folder)
    {
      string path = folder + "\\ChangesLog.txt";
      string str = string.Empty;
      if (File.Exists(path))
      {
        using (StreamReader streamReader = new StreamReader(path))
          str = streamReader.ReadLine();
        try
        {
          File.Delete(path);
        }
        catch (Exception ex)
        {
        }
      }
      if (string.IsNullOrEmpty(str))
        str = "Changes data unknown error";
      return str;
    }

    private void WriteAttributeFilterFile()
    {
    }

    private ImportToPdmsModel ReadPdmsSettingsFile(
      string settingsFile,
      ref ImportData data)
    {
      object model = new object();
      data.DataInformation = new DataInformation();
      if (!File.Exists(settingsFile))
        return (ImportToPdmsModel) null;
      if (!this.importModels.GetImportModelData(settingsFile, (object) new ImportToPdmsModel(), ref data, out model))
        return (ImportToPdmsModel) null;
      return (ImportToPdmsModel) model;
    }

    private bool StoreImportToPdmsModelInformation(
      ImportToPdmsModel importVersionData,
      ref ImportData data)
    {
      string path = data.ImportModelsFolder + "\\" + importVersionData.Name + "\\Settings\\";
      if (!Directory.Exists(path))
      {
        try
        {
          Directory.CreateDirectory(path);
        }
        catch (Exception ex)
        {
          data.Errors.ErrorCode = ErrorCode.FolderCreationFailed;
          data.Errors.ErrorInfo = path;
        }
      }
      string file = path + "Settings.xml";
      if (new ClassSerializer().ToXml((object) importVersionData, file, true))
        return true;
      data.Errors.ErrorCode = ErrorCode.ImportSettingsStoringFailed;
      data.Errors.ErrorInfo = file;
      return false;
    }

    private bool ReadPdmsVersionsAndTheirSettings(
      ref ImportData data,
      out List<ImportToPdmsModel> list,
      DateTime modelOpeningTimeUTC)
    {
      list = new List<ImportToPdmsModel>();
      if (!Directory.Exists(data.ImportModelsFolder))
      {
        try
        {
          Directory.CreateDirectory(data.ImportModelsFolder);
        }
        catch (Exception ex)
        {
          data.Errors.ErrorCode = ErrorCode.FolderCreationFailed;
          data.Errors.ErrorInfo = data.ImportModelsFolder;
          return false;
        }
      }
      foreach (DirectoryInfo directory in new DirectoryInfo(data.ImportModelsFolder).GetDirectories())
      {
        string name = directory.Name;
        if (!(name == "Settings"))
        {
          string path = data.ImportModelsFolder + "\\Settings";
          string str1 = data.ImportModelsFolder + "\\" + directory.Name + "\\Settings\\Settings.xml";
          try
          {
            if (!Directory.Exists(path))
              Directory.CreateDirectory(path);
          }
          catch (Exception ex)
          {
            data.Errors.ErrorCode = ErrorCode.FolderCreationFailed;
            data.Errors.ErrorInfo = path;
            return false;
          }
          if (File.Exists(str1))
          {
            ImportToPdmsModel importToPdmsModel1 = this.ReadPdmsSettingsFile(str1, ref data);
            if (importToPdmsModel1 != null && !list.Contains(importToPdmsModel1))
            {
              string location = importToPdmsModel1.Location;
              string datetime = string.Empty;
              if (importToPdmsModel1.DateTimeUtc != null)
                datetime = importToPdmsModel1.DateTimeUtc;
              string newDateTime = string.Empty;
              new ImportModels().FindUpdates(location, name, datetime, out newDateTime);
              importToPdmsModel1.NewDateTimeUtc = newDateTime;
              string str2 = str1 + ".bak";
              if (File.Exists(str2) && new FileInfo(str2).LastWriteTimeUtc.Ticks < modelOpeningTimeUTC.Ticks)
              {
                ImportToPdmsModel importToPdmsModel2 = this.ReadPdmsSettingsFile(str2, ref data);
                try
                {
                  File.Delete(str1);
                }
                catch
                {
                  data.Errors.ErrorCode = ErrorCode.DeletingOfFileFailed;
                  data.Errors.ErrorInfo = str1;
                  return false;
                }
                try
                {
                  File.Copy(str2, str1, true);
                  File.SetLastWriteTime(str2, DateTime.Now);
                }
                catch
                {
                  data.Errors.ErrorCode = ErrorCode.FileCopyFailed;
                  data.Errors.ErrorInfo = str2 + " to " + str1;
                  return false;
                }
                if (string.IsNullOrEmpty(importToPdmsModel1.DateTimeUtc))
                {
                  importToPdmsModel1.NewDateTimeUtc = importToPdmsModel2.NewDateTimeUtc;
                  importToPdmsModel1.DateTimeUtc = string.Empty;
                }
                else
                {
                  importToPdmsModel1.NewDateTimeUtc = importToPdmsModel1.DateTimeUtc;
                  importToPdmsModel1.DateTimeUtc = importToPdmsModel2.DateTimeUtc;
                }
              }
              if (importToPdmsModel1.NewDateTimeUtc == importToPdmsModel1.DateTimeUtc)
                importToPdmsModel1.NewDateTimeUtc = string.Empty;
              if (importToPdmsModel1.NewDateTimeUtc == null)
                importToPdmsModel1.NewDateTimeUtc = string.Empty;
              if (importToPdmsModel1.DateTimeUtc == null)
                importToPdmsModel1.DateTimeUtc = string.Empty;
              list.Add(importToPdmsModel1);
            }
          }
        }
      }
      return true;
    }

    private List<ExportToTeklaModel> ModelsList(DataTable modelsTable)
    {
      List<ExportToTeklaModel> exportToTeklaModelList = new List<ExportToTeklaModel>();
      if (modelsTable.Rows != null)
      {
        foreach (DataRow row in (InternalDataCollectionBase) modelsTable.Rows)
        {
          ExportToTeklaModel exportToTeklaModel = new ExportToTeklaModel();
          exportToTeklaModel.Name = row["Name"].ToString();
          exportToTeklaModel.DateTimeUtc = row["DateTimeUtc"].ToString();
          exportToTeklaModel.HierarchyName = row["HierarchyName"].ToString();
          exportToTeklaModel.Folder = row["Folder"].ToString();
          if (!exportToTeklaModelList.Contains(exportToTeklaModel))
            exportToTeklaModelList.Add(exportToTeklaModel);
        }
      }
      return exportToTeklaModelList;
    }

    private bool ValidateExportVersionData(ExportToTeklaModel exportVersionData)
    {
      return !string.IsNullOrEmpty(exportVersionData.Folder) && !string.IsNullOrEmpty(exportVersionData.HierarchyName) && (!string.IsNullOrEmpty(exportVersionData.Name) && !string.IsNullOrEmpty(exportVersionData.SoftwareVersion)) && exportVersionData.ApplicationVersion >= 1;
    }

    private bool ExportToTeklaFolders(
      SoftwareOptions exportSoftware,
      SoftwareOptions importSoftware,
      string rootFolder,
      ExportToTeklaModel exportVersionData,
      out ExportData data)
    {
      data = new ExportData();
      data.DataInformation = new DataInformation();
      data.Errors = new Errors();
      data.DataInformation.ExportSoftware = exportSoftware;
      data.DataInformation.ImportSoftware = importSoftware;
      data.RootFolder = rootFolder;
      ImportData importData = new ImportData();
      if (!new FolderHierarchy().HandleFolderHierarchy(rootFolder, IfcModelCollaboration.IfcModelCollaboration.Action.Export, ref importData, ref data) || string.IsNullOrEmpty(data.ExportModelsFolder))
        return false;
      if (string.IsNullOrEmpty(exportVersionData.Folder))
        exportVersionData.Folder = rootFolder + "\\ExportTo" + importSoftware.ToString();
      exportVersionData.FolderAbsolute = exportVersionData.Folder;
      if (exportVersionData.Folder.StartsWith(".\\"))
        exportVersionData.FolderAbsolute = Path.Combine(data.RootFolder, exportVersionData.Folder.Remove(0, 2));
      if (exportVersionData.Folder.Contains(rootFolder))
        exportVersionData.Folder = exportVersionData.Folder.Replace(rootFolder, ".\\");
      return true;
    }

    public bool WriteDataInformation(
      string tobezipped,
      ref ExportToTeklaModel exportVersionData,
      bool isImperial = false)
    {
      bool flag = true;
      int num = 1;
      DateTime universalTime = DateTime.Now.ToUniversalTime();
      exportVersionData.DateTimeUtc = universalTime.Year.ToString((IFormatProvider) CultureInfo.InvariantCulture) + string.Format("{0:00}", (object) universalTime.Month) + string.Format("{0:00}", (object) universalTime.Day) + "-" + string.Format("{0:00}", (object) universalTime.Hour) + string.Format("{0:00}", (object) universalTime.Minute) + string.Format("{0:00}", (object) universalTime.Second);
      exportVersionData.NameFull = exportVersionData.Name + "#" + exportVersionData.DateTimeUtc;
      DataInformation dataInformation = new DataInformation();
      dataInformation.DateTimeUtc = exportVersionData.DateTimeUtc;
      dataInformation.NameFull = exportVersionData.NameFull;
      dataInformation.Name = exportVersionData.Name;
      dataInformation.SoftwareVersion = exportVersionData.SoftwareVersion;
      dataInformation.ApplicationVersion = exportVersionData.ApplicationVersion;
      dataInformation.LinkVersion = num;
      Errors errors = (Errors) null;
      if (!this.ifcModelCollaboration.WriteDataInformationXml(SoftwareOptions.PDMS, SoftwareOptions.TeklaStructures, tobezipped, ref dataInformation, ref errors, isImperial))
        flag = false;
      return flag;
    }

    public bool StoreExportToTeklaModelInformation(
      ExportToTeklaModel exportVersionData,
      ref ExportData data)
    {
      this.exportModels.GetModelSpecificData(exportVersionData.Name, ref data);
      string exportModelDataFolder = data.ExportModelDataFolder;
      if (!Directory.Exists(exportModelDataFolder))
      {
        try
        {
          Directory.CreateDirectory(exportModelDataFolder);
        }
        catch (Exception ex)
        {
          data.Errors.ErrorCode = ErrorCode.FolderCreationFailed;
          data.Errors.ErrorInfo = exportModelDataFolder;
          return false;
        }
      }
      if (!this.ValidateExportVersionData(exportVersionData))
      {
        data.Errors.ErrorCode = ErrorCode.DataMissing;
        data.Errors.ErrorInfo = nameof (exportVersionData);
        return false;
      }
      string modelSettingsFile = data.ExportModelSettingsFile;
      if (new ClassSerializer().ToXml((object) exportVersionData, modelSettingsFile, false))
        return true;
      data.Errors.ErrorCode = ErrorCode.ExportSettingsStoringFailed;
      data.Errors.ErrorInfo = modelSettingsFile;
      return false;
    }
  }
}
