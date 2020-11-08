// Decompiled with JetBrains decompiler
// Type: IfcModelCollaboration.Profiles
// Assembly: IfcModelCollaboration, Version=2099.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4FD7DE9B-FBF7-4747-A620-F10F7C8F2B9D
// Assembly location: C:\TS_E3D\2.1\TS-E3D_Library\IfcModelCollaboration.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace IfcModelCollaboration
{
  internal class Profiles
  {
    private readonly List<string> header = new List<string>()
    {
      "//Tekla Structures;PDMS",
      "//HE300A;HEA300 -> Libarary profile mapping",
      "//L:B:*:H:*:T;DESPAR_SPEC/L:B:H:T - > parametric mapping",
      "//L200*100*10;DESPAR_SPEC/L:200:100:10 -> profile to parametric profile mapping",
      "//UB:H:*:B:*:T;DESPAR_SPEC/UB:H:B:T% -> % adds .0",
      "//UB:H:*:B:*:T;DESPAR_SPEC/UB:H:B:T:kg/m -> % adds kg/m"
    };

    internal bool GetMappedInfo(
      bool isImp,
      string profileMappingFile,
      List<string> profiles,
      ref Dictionary<string, string> libraryProfiles,
      ref Dictionary<string, Tuple<string, List<string>>> parametricProfiles,
      out Errors errors,
      out Dictionary<string, string> profileMapping,
      out DataTable profileMappingTable)
    {
      errors = new Errors();
      profileMapping = (Dictionary<string, string>) null;
      profileMappingTable = (DataTable) null;
      if (!File.Exists(profileMappingFile))
      {
        errors.ErrorCode = ErrorCode.FileDoesNotExist;
        errors.ErrorInfo = profileMappingFile;
        return false;
      }
      Dictionary<string, string> libraryProfiles1 = new Dictionary<string, string>();
      Dictionary<string, Tuple<string, string>> parametricProfiles1 = new Dictionary<string, Tuple<string, string>>();
      if (libraryProfiles == null)
        libraryProfiles = new Dictionary<string, string>();
      if (parametricProfiles == null)
        parametricProfiles = new Dictionary<string, Tuple<string, List<string>>>();
      List<string> libraryToParametricProfiles;
      this.ReadProfileMappingFile(profileMappingFile, ref libraryProfiles1, out libraryToParametricProfiles, ref parametricProfiles1, ref errors, out profileMapping);
      this.MapProfiles(isImp, (IEnumerable<string>) profiles, libraryProfiles1, parametricProfiles1, ref libraryProfiles, libraryToParametricProfiles, ref parametricProfiles, ref errors, out profileMappingTable);
      return errors.ErrorCode == ErrorCode.None;
    }

    internal bool GetProfileMapping(
      string profileMappingFile,
      out Dictionary<string, string> mapped)
    {
      Dictionary<string, string> libraryProfiles = (Dictionary<string, string>) null;
      Dictionary<string, Tuple<string, string>> parametricProfiles = (Dictionary<string, Tuple<string, string>>) null;
      Errors errors = (Errors) null;
      mapped = new Dictionary<string, string>();
      List<string> libraryToParametricProfiles;
      return this.ReadProfileMappingFile(profileMappingFile, ref libraryProfiles, out libraryToParametricProfiles, ref parametricProfiles, ref errors, out mapped);
    }

    internal bool SaveProfileMapping(
      string profileMappingFile,
      Dictionary<string, string> profileMapping,
      out Errors errors)
    {
      errors = new Errors();
      try
      {
        using (StreamWriter streamWriter = new StreamWriter(profileMappingFile))
        {
          foreach (string str in this.header)
            streamWriter.WriteLine(str);
          if (profileMapping != null)
          {
            foreach (KeyValuePair<string, string> keyValuePair in profileMapping)
              streamWriter.WriteLine(keyValuePair.Key + ";" + keyValuePair.Value);
          }
        }
      }
      catch (Exception ex)
      {
        errors.ErrorCode = ErrorCode.FileCreationFailed;
        return false;
      }
      return true;
    }

    private void MapProfiles(
      bool isImp,
      IEnumerable<string> profiles,
      Dictionary<string, string> library,
      Dictionary<string, Tuple<string, string>> parametric,
      ref Dictionary<string, string> libraryProfiles,
      List<string> libraryToParametric,
      ref Dictionary<string, Tuple<string, List<string>>> parametricProfiles,
      ref Errors errors,
      out DataTable profileMappingTable)
    {
      profileMappingTable = this.CreateProfileMappingTable();
      foreach (string profile in profiles)
      {
        string upper = profile.ToUpper(CultureInfo.InvariantCulture);
        try
        {
          if (library.ContainsKey(upper) && !libraryProfiles.ContainsKey(upper))
          {
            string str;
            library.TryGetValue(upper, out str);
            if (!string.IsNullOrEmpty(str))
            {
              libraryProfiles.Add(upper, str);
              profileMappingTable.Rows.Add((object) upper, (object) str, (object) upper, (object) str);
              continue;
            }
          }
          string prefix = upper;
          if (!libraryToParametric.Contains(upper))
            prefix = this.ParseProfilePrefix(upper);
          int parameterNumber = this.ParseParameterNumber(upper.Remove(0, prefix.Length));
          string str1 = prefix;
          if (parameterNumber > 0)
            str1 = str1 + ":" + parameterNumber.ToString();
          Dictionary<string, Tuple<string, string>> paraList = new Dictionary<string, Tuple<string, string>>();
          foreach (string key in parametric.Keys)
          {
            if (key.StartsWith(str1))
            {
              Tuple<string, string> tuple;
              parametric.TryGetValue(key, out tuple);
              paraList.Add(key, tuple);
            }
          }
          this.FindProfileCategory(isImp, paraList, upper, prefix, ref parametricProfiles, ref profileMappingTable, ref libraryProfiles);
          if (profileMappingTable.Rows.Find((object) upper) == null)
            profileMappingTable.Rows.Add((object) upper, (object) string.Empty, (object) upper, (object) string.Empty);
        }
        catch (Exception ex)
        {
          errors.ErrorCode = ErrorCode.ReadingOfDataFailed;
          errors.ErrorInfo = upper;
        }
      }
    }

    private void FindProfileCategory(
      bool isImp,
      Dictionary<string, Tuple<string, string>> paraList,
      string profile,
      string prefix,
      ref Dictionary<string, Tuple<string, List<string>>> parametricProfiles,
      ref DataTable profileMappingTable,
      ref Dictionary<string, string> libraryProfiles)
    {
      foreach (string key in paraList.Keys)
      {
        if (!parametricProfiles.ContainsKey(profile))
        {
          Tuple<string, string> tuple;
          paraList.TryGetValue(key, out tuple);
          string[] ts = tuple.Item1.Split(':');
          string[] pdms = tuple.Item2.Split(':');
          string temp = profile;
          if (!string.IsNullOrEmpty(prefix))
            temp = profile.Replace(prefix, string.Empty);
          Dictionary<string, string> map = new Dictionary<string, string>();
          if (this.CheckTsProfileData(ts, temp, ref map))
          {
            List<string> mapping = new List<string>();
            string mapString = pdms[0];
            for (int index1 = 1; index1 < pdms.Length; ++index1)
            {
              string str1 = pdms[index1];
              string str2 = string.Empty;
              bool flag = false;
              int num1 = 0;
              if (str1.EndsWith("%"))
              {
                num1 = str1.Count<char>((Func<char, bool>) (c => c == '%'));
                flag = true;
                str1 = str1.Replace("%", string.Empty);
              }
              if (map.ContainsKey(str1))
              {
                map.TryGetValue(str1, out str2);
                if (!string.IsNullOrEmpty(str2))
                {
                  if (flag && !str2.Contains(".") && !str2.Contains(","))
                  {
                    str2 += ".";
                    for (int index2 = 0; index2 < num1; ++index2)
                      str2 += "0";
                  }
                  else if (flag)
                  {
                    int num2 = str2.LastIndexOf('.');
                    if (num2 < 1)
                      num2 = str2.LastIndexOf(',');
                    int num3 = num2 + 1;
                    if (num3 < str2.Length - num1)
                      str2 = str2.Remove(num3 + num1);
                    else if (num3 > str2.Length - num1)
                    {
                      int num4 = num3 - (str2.Length - num1);
                      for (int index2 = 0; index2 < num4; ++index2)
                        str2 += "0";
                    }
                  }
                  mapping.Add(str2);
                  mapString = mapString + ":" + str2;
                }
              }
              else
              {
                mapping.Add(str1);
                mapString = mapString + ":" + str1;
              }
            }
            this.CategorizeProfiles(isImp, pdms, mapping, profile, mapString, tuple, ref parametricProfiles, ref profileMappingTable, ref libraryProfiles);
          }
        }
      }
    }

    private void CategorizeProfiles(
      bool isImp,
      string[] pdms,
      List<string> mapping,
      string profile,
      string mapString,
      Tuple<string, string> tuple,
      ref Dictionary<string, Tuple<string, List<string>>> parametricProfiles,
      ref DataTable profileMappingTable,
      ref Dictionary<string, string> libraryProfiles)
    {
      bool flag = false;
      List<string> stringList = new List<string>();
      for (int index = 0; index < mapping.Count; ++index)
      {
        double d;
        if (!isImp && this.GetDouble(mapping[index], out d))
          flag = true;
        else if (this.IsImperial(mapping[index], out d))
        {
          flag = true;
          isImp = true;
          stringList.Add(d.ToString((IFormatProvider) CultureInfo.InvariantCulture));
        }
        else
        {
          flag = false;
          break;
        }
      }
      if (flag)
      {
        if (isImp)
          mapping = stringList;
        Tuple<string, List<string>> tuple1 = new Tuple<string, List<string>>(pdms[0], mapping);
        parametricProfiles.Add(profile, tuple1);
        profileMappingTable.Rows.Add((object) profile, (object) mapString, (object) tuple.Item1, (object) tuple.Item2);
      }
      else
      {
        if (libraryProfiles.ContainsKey(profile))
          return;
        profileMappingTable.Rows.Add((object) profile, (object) mapString.Replace(":", string.Empty), (object) tuple.Item1, (object) tuple.Item2);
        libraryProfiles.Add(profile, mapString.Replace(":", string.Empty));
      }
    }

    private bool IsImperial(string p, out double d)
    {
      d = 0.0;
      foreach (char c in p)
      {
        if (!char.IsNumber(c) && c != '"' && (c != ' ' && !(c.ToString((IFormatProvider) CultureInfo.InvariantCulture) == "'")) && c != '/')
          return false;
      }
      new UnitConverterLocally(true).TryConvertFromCurrentUnitsToMm(p, out d);
      return true;
    }

    private bool CheckTsProfileData(string[] ts, string temp, ref Dictionary<string, string> map)
    {
      int length = ts.Length;
      if (length != 2 || !(ts[1] == string.Empty))
      {
        bool flag = false;
        for (int index = 1; index < length; ++index)
        {
          if (index % 2 == 0)
          {
            temp = temp.Remove(0, ts[index].Length);
          }
          else
          {
            string t = ts[index];
            int num = 0;
            string str = temp;
            if (index < ts.Length - 1)
            {
              num = temp.IndexOf(ts[index + 1].ToString((IFormatProvider) CultureInfo.InvariantCulture), StringComparison.Ordinal);
              if (num < 1)
              {
                flag = true;
                break;
              }
              str = temp.Remove(num);
            }
            if (map.ContainsKey(t))
              return false;
            map.Add(t, str);
            if (num <= temp.Length)
              temp = temp.Remove(0, num);
          }
        }
        if (flag)
          return false;
      }
      return true;
    }

    private DataTable CreateProfileMappingTable()
    {
      DataTable dataTable = new DataTable();
      DataColumn[] dataColumnArray = new DataColumn[1];
      DataColumn column = new DataColumn();
      column.DataType = Type.GetType("System.String");
      column.ColumnName = "Tekla";
      dataTable.Columns.Add(column);
      dataColumnArray[0] = column;
      dataTable.Columns.Add("Pdms", typeof (string));
      dataTable.Columns.Add("TeklaRule", typeof (string));
      dataTable.Columns.Add("PdmsRule", typeof (string));
      dataTable.PrimaryKey = dataColumnArray;
      return dataTable;
    }

    private void ReadProfilesFile(
      string profilesFile,
      out List<string> profiles,
      out Dictionary<string, Dictionary<string, double>> parameters)
    {
      profiles = new List<string>();
      parameters = new Dictionary<string, Dictionary<string, double>>();
      if (this.ReadProfilesFileDataTable(profilesFile, out profiles))
        return;
      this.ReadProfilesFileDataSet(profilesFile, out profiles, out parameters);
    }

    private bool ReadProfilesFileDataTable(string profilesFile, out List<string> profiles)
    {
      profiles = new List<string>();
      DataTable dataTable = new DataTable(nameof (Profiles));
      if (!new XmlHandler().ReadXmlToDataTable(profilesFile, nameof (Profiles), out dataTable))
        return false;
      foreach (object row in (InternalDataCollectionBase) dataTable.Rows)
      {
        DataRow dataRow = row as DataRow;
        if (dataRow != null)
        {
          try
          {
            string str = dataRow["ProfileName"].ToString();
            if (!profiles.Contains(str))
              profiles.Add(str);
          }
          catch (Exception ex)
          {
          }
        }
      }
      return true;
    }

    private void ReadProfilesFileDataSet(
      string profilesFile,
      out List<string> profiles,
      out Dictionary<string, Dictionary<string, double>> parameters)
    {
      profiles = new List<string>();
      parameters = new Dictionary<string, Dictionary<string, double>>();
      DataSet dataSet;
      if (!new XmlHandler().ReadXmlToDataSet(profilesFile, out dataSet))
        return;
      foreach (object row in (InternalDataCollectionBase) dataSet.Tables[nameof (Profiles)].Rows)
      {
        DataRow dataRow = row as DataRow;
        if (dataRow != null)
        {
          string key = dataRow["ProfileName"].ToString();
          if (!profiles.Contains(key))
            profiles.Add(key);
          DataTable table = dataSet.Tables[key];
          if (table != null)
          {
            IEnumerator enumerator = table.Rows.GetEnumerator();
            Dictionary<string, double> dictionary = new Dictionary<string, double>();
            while (enumerator.MoveNext())
            {
              DataRow current = enumerator.Current as DataRow;
              if (current != null)
                dictionary.Add(current["Key"].ToString(), double.Parse(current["Value"].ToString(), (IFormatProvider) CultureInfo.InvariantCulture));
            }
            if (!parameters.ContainsKey(key))
              parameters.Add(key, dictionary);
          }
        }
      }
    }

    private bool GetDouble(string text, out double d)
    {
      d = 0.0;
      try
      {
        d = double.Parse(text, (IFormatProvider) CultureInfo.InvariantCulture);
      }
      catch (Exception ex)
      {
        return false;
      }
      return true;
    }

    private bool ReadProfileMappingFile(
      string profileMappingFile,
      ref Dictionary<string, string> libraryProfiles,
      out List<string> libraryToParametricProfiles,
      ref Dictionary<string, Tuple<string, string>> parametricProfiles,
      ref Errors errors,
      out Dictionary<string, string> profileMapping)
    {
      libraryProfiles = new Dictionary<string, string>();
      libraryToParametricProfiles = new List<string>();
      parametricProfiles = new Dictionary<string, Tuple<string, string>>();
      profileMapping = new Dictionary<string, string>();
      try
      {
        if (!this.DetectUTF8Encoding(profileMappingFile))
        {
          string contents = File.ReadAllText(profileMappingFile, Encoding.UTF8);
          File.WriteAllText(profileMappingFile, contents, Encoding.UTF8);
          if (!this.DetectUTF8Encoding(profileMappingFile))
          {
            errors.ErrorCode = ErrorCode.FileEncodingIsNotUTF8;
            errors.ErrorInfo = profileMappingFile;
            return false;
          }
        }
      }
      catch (Exception ex)
      {
        errors.ErrorCode = ErrorCode.FileEncodingIsNotUTF8;
        errors.ErrorInfo = profileMappingFile;
        return false;
      }
      try
      {
        using (StreamReader streamReader = new StreamReader(profileMappingFile))
        {
          string str;
          while ((str = streamReader.ReadLine()) != null)
          {
            str.TrimStart();
            str.TrimEnd();
            if (!str.StartsWith("/"))
            {
              string[] strArray1 = str.Split(';');
              if (strArray1.Length != 2)
              {
                if (str != string.Empty)
                {
                  errors.ErrorCode = ErrorCode.DataInformationReadingFailed;
                  errors.ErrorInfo = str;
                }
              }
              else
              {
                string upper = strArray1[0].ToUpper();
                string second = strArray1[1];
                upper.TrimEnd();
                second.TrimStart();
                if (!upper.Contains(":") && !second.Contains(":"))
                {
                  if (!libraryProfiles.ContainsKey(upper))
                    libraryProfiles.Add(upper, second);
                }
                else
                {
                  string[] strArray2 = upper.Split(':');
                  int num = strArray2.Length - 1;
                  string key = strArray2[0];
                  if (strArray2.Length > 1)
                    key = key + ":" + num.ToString() + ":" + upper.Replace(":", string.Empty);
                  if (!parametricProfiles.ContainsKey(key))
                  {
                    parametricProfiles.Add(key, new Tuple<string, string>(upper, second));
                    if (!upper.Contains(":") && second.Contains(":") && !libraryToParametricProfiles.Contains(key))
                      libraryToParametricProfiles.Add(key);
                  }
                }
                if (!profileMapping.ContainsKey(upper))
                  profileMapping.Add(upper, second);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        errors.ErrorCode = ErrorCode.DataInformationReadingFailed;
        errors.ErrorInfo = profileMappingFile;
        return false;
      }
      return true;
    }

    private bool DetectUTF8Encoding(string filename)
    {
      byte[] bytes = File.ReadAllBytes(filename);
      try
      {
        Encoding.GetEncoding("UTF-8", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback).GetString(bytes);
        return true;
      }
      catch
      {
        return false;
      }
    }

    private string ParseProfilePrefix(string profile)
    {
      string empty = string.Empty;
      foreach (char c in profile)
      {
        if (char.IsLetter(c) || char.IsPunctuation(c) || (c == ' ' || c == '='))
          empty += c.ToString();
        else
          break;
      }
      return empty;
    }

    private string ParseProfileNumber(string profile)
    {
      string empty = string.Empty;
      foreach (char c in profile)
      {
        if (char.IsNumber(c) || c == '.' || (c == ',' || c == '"') || (c == ' ' || c.ToString((IFormatProvider) CultureInfo.InvariantCulture) == "'" || c == '/'))
          empty += c.ToString();
        else
          break;
      }
      return empty;
    }

    private int ParseParameterNumber(string profile)
    {
      int num1 = 0;
      while (!string.IsNullOrEmpty(profile) && !string.IsNullOrEmpty(profile))
      {
        string profileNumber = this.ParseProfileNumber(profile);
        profile = string.IsNullOrEmpty(profile) ? string.Empty : profile.Remove(0, profileNumber.Length);
        int num2 = num1 + 1;
        if (string.IsNullOrEmpty(profile))
          return num2;
        string profilePrefix = this.ParseProfilePrefix(profile);
        profile = string.IsNullOrEmpty(profile) ? string.Empty : profile.Remove(0, profilePrefix.Length);
        num1 = num2 + 1;
        if (num1 == 100)
          return num1;
      }
      return num1;
    }
  }
}
