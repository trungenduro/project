// Decompiled with JetBrains decompiler
// Type: IfcModelCollaboration.Tools
// Assembly: IfcModelCollaboration, Version=2099.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4FD7DE9B-FBF7-4747-A620-F10F7C8F2B9D
// Assembly location: C:\TS_E3D\2.1\TS-E3D_Library\IfcModelCollaboration.dll

using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IfcModelCollaboration
{
  internal class Tools
  {
    public bool ZipFiles(
      string tobezipped,
      string folder,
      string fileName,
      Dictionary<string, string> brepsDictionary,
      ref List<string> brepErrors,
      SoftwareOptions software,
      string extension,
      double revitVersion)
    {
      bool flag = false;
      if (brepErrors == null)
        brepErrors = new List<string>();
      string str1 = folder + "\\" + fileName + extension;
      if (!Directory.Exists(folder))
      {
        try
        {
          Directory.CreateDirectory(folder);
        }
        catch (Exception ex)
        {
          return false;
        }
      }
      List<string> stringList = new List<string>();
      try
      {
        if (File.Exists(str1))
          File.Delete(str1);
        FileInfo[] files = new DirectoryInfo(tobezipped).GetFiles();
        using (ZipFile zipFile = new ZipFile(str1)
        {
          UseZip64WhenSaving = Zip64Option.AsNecessary
        })
        {
          zipFile.AlternateEncodingUsage = ZipOption.AsNecessary;
          zipFile.AlternateEncoding = Encoding.UTF8;
          foreach (FileInfo fileInfo in files)
          {
            if (software != SoftwareOptions.ArchiCAD || !(fileInfo.Extension.ToUpper() != ".ifc".ToUpper()))
            {
              zipFile.AddFile(fileInfo.FullName, string.Empty);
              if (fileInfo.Extension.ToUpper() == ".ifc".ToUpper())
                flag = true;
              if (fileInfo.Extension.ToUpper() == ".skp".ToUpper())
              {
                string str2 = fileInfo.Name.Replace(".skp", string.Empty);
                if (!stringList.Contains(str2))
                  stringList.Add(str2);
              }
              if (fileInfo.Extension.ToUpper() == ".dwg".ToUpper())
              {
                string str2 = fileInfo.Name.Replace(".dwg", string.Empty);
                if (!stringList.Contains(str2))
                  stringList.Add(str2);
              }
              if (fileInfo.Extension.ToUpper() == ".dgn".ToUpper())
              {
                string name = fileInfo.Name;
                if (!stringList.Contains(name))
                  stringList.Add(name);
              }
            }
          }
          zipFile.Save();
        }
      }
      catch (Exception ex)
      {
        brepErrors = (List<string>) null;
        return false;
      }
      if (software == SoftwareOptions.Revit && revitVersion < 2017.1)
      {
        foreach (KeyValuePair<string, string> breps in brepsDictionary)
        {
          string str2 = breps.Value;
          if (!stringList.Contains(str2) && !brepErrors.Contains(str2))
            brepErrors.Add(str2);
        }
      }
      if (software == SoftwareOptions.PDMS && brepsDictionary.Count > 0 && stringList.Count == 0)
        brepErrors.Add("dgn file of breps missing");
      return flag;
    }

    public bool ZipFilesToTekla(
      string tobezipped,
      string folder,
      string fileName,
      string extension)
    {
      bool flag = false;
      string str = folder + "\\" + fileName + extension;
      if (!Directory.Exists(folder))
      {
        try
        {
          Directory.CreateDirectory(folder);
        }
        catch (Exception ex)
        {
          return false;
        }
      }
      List<string> stringList = new List<string>();
      try
      {
        if (File.Exists(str))
          File.Delete(str);
        FileInfo[] files = new DirectoryInfo(tobezipped).GetFiles();
        using (ZipFile zipFile = new ZipFile(str)
        {
          UseZip64WhenSaving = Zip64Option.AsNecessary
        })
        {
          zipFile.AlternateEncodingUsage = ZipOption.AsNecessary;
          zipFile.AlternateEncoding = Encoding.UTF8;
          foreach (FileInfo fileInfo in files)
          {
            zipFile.AddFile(fileInfo.FullName, string.Empty);
            if (fileInfo.Extension.ToUpper() == ".ifc".ToUpper())
              flag = true;
          }
          zipFile.Save();
        }
      }
      catch (Exception ex)
      {
        return false;
      }
      return flag;
    }

    internal bool CreateTempExportFolder(out string tempFolder)
    {
      tempFolder = Path.GetTempPath() + "\\" + DateTime.Now.Ticks.ToString((IFormatProvider) CultureInfo.InvariantCulture) + "\\";
      try
      {
        Directory.CreateDirectory(tempFolder);
      }
      catch (Exception ex)
      {
        return false;
      }
      return true;
    }

    internal string MakePacketUnique(string text, string text2)
    {
      return new Guid(MD5.Create().ComputeHash(Encoding.Default.GetBytes(text + text2 + "wearethechampions"))).ToString();
    }

    internal DataSet ReadXmlToDataSet(string file, ref ImportData importData)
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
          importData.Errors.ErrorCode = ErrorCode.DataMissing;
          importData.Errors.ErrorInfo = "Writing changes data to dataset failed";
          return (DataSet) null;
        }
      }
      else
      {
        importData.Errors.ErrorCode = ErrorCode.DataMissing;
        importData.Errors.ErrorInfo = "Changes data missing";
      }
      return dataSet;
    }

    internal void CleanOldVersions(
      ImportData data,
      string name,
      string newVersion,
      string previousVersion)
    {
      foreach (DirectoryInfo directory in new DirectoryInfo(data.ImportModelFolder).GetDirectories())
      {
        if (!(directory.Name == "Caches") && !(directory.Name == "Settings") && !directory.Name.Contains(newVersion))
        {
          if (!directory.Name.Contains(previousVersion))
          {
            try
            {
              Directory.Delete(directory.FullName, true);
            }
            catch (Exception ex)
            {
            }
          }
        }
      }
      DirectoryInfo directoryInfo = new DirectoryInfo(data.ImportModelFolder + "\\Caches");
      foreach (FileInfo file in directoryInfo.GetFiles("*.xml"))
      {
        try
        {
          File.Delete(file.FullName);
        }
        catch (Exception ex)
        {
        }
      }
      foreach (FileInfo file in directoryInfo.GetFiles("*.tsfo"))
      {
        if (!file.Name.Contains(newVersion))
        {
          if (!file.Name.Contains(previousVersion))
          {
            try
            {
              File.Delete(file.FullName);
            }
            catch (Exception ex)
            {
            }
          }
        }
      }
      foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
      {
        if (!directory.Name.Contains(newVersion))
        {
          if (!directory.Name.Contains(previousVersion))
          {
            try
            {
              Directory.Delete(directory.FullName, true);
            }
            catch (Exception ex)
            {
            }
          }
        }
      }
    }
  }
}
