﻿#if !(UNITY_WEBPLAYER || UNITY_WINRT || UNITY_WII || UNITY_PS4 || UNITY_WSA)
#define CAN_HANDLE_SCREENSHOTS
#endif

#if UNITY_WEBPLAYER || UNITY_WINRT || UNITY_WII || UNITY_PS4 || UNITY_WSA
#define SAVE_IN_PLAYERPREFS
#endif

using UnityEngine;
using System.Collections.Generic;
using System.IO;


namespace AC
{

	#if !SAVE_IN_PLAYERPREFS

	/** A save-file handler that stores save-games as separate files on the machine */
	public class SaveFileHandler_SystemFile : iSaveFileHandler
	{

		#region PublicFunctions

		public virtual string GetDefaultSaveLabel (int saveID)
		{
			string label = (saveID == 0)
							? SaveSystem.AutosaveLabel
							: (SaveSystem.SaveLabel + " " + saveID.ToString ());

			label += GetTimeString (System.DateTime.Now);
			return label;
		}


		public virtual void DeleteAll (int profileID)
		{
			GatherSaveFiles (profileID, OnGatherFilesForDeletion);
		}


		public virtual void Delete (SaveFile saveFile, System.Action<bool> callback = null)
		{
			string filename = saveFile.fileName;

			if (!string.IsNullOrEmpty (filename))
			{
				FileInfo t = new FileInfo (filename);
				if (t.Exists)
				{
					t.Delete ();

					#if CAN_HANDLE_SCREENSHOTS
					if (KickStarter.settingsManager.saveScreenshots == SaveScreenshots.Always || (KickStarter.settingsManager.saveScreenshots == SaveScreenshots.ExceptWhenAutosaving && !saveFile.IsAutoSave))
					{
						DeleteScreenshot (saveFile.screenshotFilename);
					}
					#endif

					ACDebug.Log ("File deleted: " + filename);
					if (callback != null) callback.Invoke (true);
					return;
				}
			}

			if (callback != null) callback.Invoke (false);
		}


		public virtual void Save (SaveFile saveFile, string dataToSave, System.Action<bool> callback)
		{
			string fullFilename = GetSaveDirectory () + Path.DirectorySeparatorChar.ToString () + GetSaveFilename (saveFile.saveID, saveFile.profileID);
			bool isSuccessful = false;

			try
			{
				StreamWriter writer;
				FileInfo t = new FileInfo (fullFilename);
				
				if (!t.Exists)
				{
					writer = t.CreateText ();
				}
				else
				{
					t.Delete ();
					writer = t.CreateText ();
				}
				
				writer.Write (dataToSave);
				writer.Close ();

				ACDebug.Log ("File written: " + fullFilename);
				isSuccessful = true;
			}
			catch (System.Exception e)
 			{
				ACDebug.LogWarning ("Could not save data to file '" + fullFilename + "'. Exception: " + e);
 			}

			callback.Invoke (isSuccessful);
		}


		public virtual void Load (SaveFile saveFile, bool doLog, System.Action<SaveFile, string> callback)
		{
			string _data = string.Empty;
			
			if (File.Exists (saveFile.fileName))
			{
				StreamReader r = File.OpenText (saveFile.fileName);
				
				string _info = r.ReadToEnd ();
				r.Close ();
				_data = _info;
			}
			
			if (doLog && !string.IsNullOrEmpty (_data))
			{
				ACDebug.Log ("File read: " + saveFile.fileName);
			}

			callback.Invoke (saveFile, _data);
		}


		public virtual bool SupportsSaveThreading ()
		{
			return true;
		}


		public virtual void GatherSaveFiles (int profileID, System.Action<List<SaveFile>> callback)
		{
			GatherSaveFiles (profileID, false, -1, string.Empty, string.Empty, callback);
		}


		public virtual void GatherImportFiles (int profileID, int boolID, string separateProjectName, string separateFilePrefix, System.Action<List<SaveFile>> callback)
		{
			if (!string.IsNullOrEmpty (separateProjectName) && !string.IsNullOrEmpty (separateFilePrefix))
			{
				GatherSaveFiles (profileID, true, boolID, separateProjectName, separateFilePrefix, callback);
			}
		}


		public virtual SaveFile GetSaveFile (int saveID, int profileID)
		{
			return GetSaveFile (saveID, profileID, false, -1, string.Empty, string.Empty);
		}


		public virtual void SaveScreenshot (SaveFile saveFile)
		{
			#if CAN_HANDLE_SCREENSHOTS
			if (saveFile.screenShot != null)
			{
				string fullFilename = GetSaveDirectory () + Path.DirectorySeparatorChar.ToString () + GetSaveFilename (saveFile.saveID, saveFile.profileID, ".jpg");

				byte[] bytes = saveFile.screenShot.EncodeToJPG ();
				File.WriteAllBytes (fullFilename, bytes);
				ACDebug.Log ("Saved screenshot: " + fullFilename);
			}
			else
			{
				ACDebug.LogWarning ("Cannot save screenshot - SaveFile's screenshot variable is null.");
			}
			#endif
		}

		#endregion


		#region ProtectedFunctions

		protected virtual SaveFile GetSaveFile (int saveID, int profileID, bool isImport, int boolID, string separateProductName, string separateFilePrefix, FileInfo[] info = null)
		{
			string saveDirectory = GetSaveDirectory (separateProductName);
			string filePrefix = (isImport) ? separateFilePrefix : KickStarter.settingsManager.SavePrefix;

			string filename = filePrefix + SaveSystem.GenerateSaveSuffix (saveID, profileID);
			string filenameWithExtention = filename + SaveSystem.GetSaveExtension ();
			string fullFilename = saveDirectory + Path.DirectorySeparatorChar.ToString () + filenameWithExtention;

			if (info == null)
			{
				DirectoryInfo dir = new DirectoryInfo (saveDirectory);
				info = dir.GetFiles (filenameWithExtention);
			}

			if (info == null)
			{
				return null;
			}

			foreach (FileInfo fileInfo in info)
			{
				if (fileInfo.Name != filenameWithExtention)
				{
					continue;
				}

				if (isImport && boolID >= 0)
				{
					string allData = LoadFile (fullFilename, false);
					if (!KickStarter.saveSystem.DoImportCheck (allData, boolID))
					{
						return null;
					}
				}

				int updateTime = 0;
				bool isAutoSave = false;
				string label = (isImport ? SaveSystem.ImportLabel : SaveSystem.SaveLabel) + " " + saveID.ToString ();
				if (saveID == 0)
				{
					label = SaveSystem.AutosaveLabel;
					isAutoSave = true;
				}

				System.TimeSpan t = fileInfo.LastWriteTime - new System.DateTime (2015, 1, 1);
				updateTime = (int) t.TotalSeconds;

				if (KickStarter.settingsManager.saveTimeDisplay != SaveTimeDisplay.None)
				{
					label += GetTimeString (fileInfo.LastWriteTime);
				}

				Texture2D screenShot = null;
				string screenshotFilename = string.Empty;
				if (KickStarter.settingsManager.saveScreenshots == SaveScreenshots.Always || (KickStarter.settingsManager.saveScreenshots == SaveScreenshots.ExceptWhenAutosaving && !isAutoSave))
				{
					screenshotFilename = saveDirectory + Path.DirectorySeparatorChar.ToString () + filename + ".jpg";
					screenShot = LoadScreenshot (screenshotFilename);
				}

				return new SaveFile (saveID, profileID, label, fullFilename, screenShot, screenshotFilename, updateTime);
			}

			return null;
		}


		protected virtual void GatherSaveFiles (int profileID, bool isImport, int boolID, string separateProductName, string separateFilePrefix, System.Action<List<SaveFile>> callback)
		{
			List<SaveFile> gatheredFiles = new List<SaveFile> ();

			string saveDirectory = GetSaveDirectory (separateProductName);
			DirectoryInfo dir = new DirectoryInfo (saveDirectory);
			FileInfo[] info = dir.GetFiles ("*" + SaveSystem.GetSaveExtension ());

			for (int i = 0; i < MaxSaves; i++)
			{
				SaveFile saveFile = GetSaveFile (i, profileID, isImport, boolID, separateProductName, separateFilePrefix, info);
				if (saveFile != null)
				{
					gatheredFiles.Add (saveFile);
				}
			}

			callback?.Invoke (gatheredFiles);
		}


		protected virtual void DeleteScreenshot (string sceenshotFilename)
		{
			#if CAN_HANDLE_SCREENSHOTS
			if (File.Exists (sceenshotFilename))
			{
				File.Delete (sceenshotFilename);
			}
			#endif
		}


		protected virtual Texture2D LoadScreenshot (string fileName)
		{
			#if CAN_HANDLE_SCREENSHOTS
			if (File.Exists (fileName) && Application.isPlaying && KickStarter.saveSystem)
			{
				byte[] bytes = File.ReadAllBytes (fileName);
				Texture2D screenshotTex = new Texture2D (KickStarter.saveSystem.ScreenshotWidth, KickStarter.saveSystem.ScreenshotHeight, TextureFormat.RGB24, false, KickStarter.settingsManager.linearColorTextures);
				screenshotTex.LoadImage (bytes);
				screenshotTex.Compress (true);
				return screenshotTex;
			}
			#endif
			return null;
		}


		protected virtual string GetSaveFilename (int saveID, int profileID = -1, string extensionOverride = "")
		{
			if (profileID == -1)
			{
				profileID = Options.GetActiveProfileID ();
			}

			string extension = (!string.IsNullOrEmpty (extensionOverride)) ? extensionOverride : SaveSystem.GetSaveExtension ();
			return KickStarter.settingsManager.SavePrefix + SaveSystem.GenerateSaveSuffix (saveID, profileID) + extension;
		}


		protected virtual string GetSaveDirectory (string separateProjectName = "")
		{
			string normalSaveDirectory = SaveSystem.PersistentDataPath;

			if (!string.IsNullOrEmpty (separateProjectName))
			{
				string[] s = normalSaveDirectory.Split ('/');
				string currentProjectName = s[s.Length - 1];
				return normalSaveDirectory.Replace (currentProjectName, separateProjectName);
			}

			return normalSaveDirectory;
		}


		protected virtual string GetTimeString (System.DateTime dateTime)
		{
			if (KickStarter.settingsManager && KickStarter.settingsManager.saveTimeDisplay != SaveTimeDisplay.None)
			{
				if (KickStarter.settingsManager.saveTimeDisplay == SaveTimeDisplay.CustomFormat)
				{
					string creationTime = dateTime.ToString (KickStarter.settingsManager.customSaveFormat);
					return " (" + creationTime + ")";
				}
				else
				{
					string creationTime = dateTime.ToShortDateString ();
					if (KickStarter.settingsManager.saveTimeDisplay == SaveTimeDisplay.TimeAndDate)
					{
						creationTime += " " + dateTime.ToShortTimeString ();
					}
					return " (" + creationTime + ")";
				}
			}

			return string.Empty;
		}


		protected virtual string LoadFile (string fullFilename, bool doLog = true)
		{
			string _data = string.Empty;
			
			if (File.Exists (fullFilename))
			{
				StreamReader r = File.OpenText (fullFilename);

				string _info = r.ReadToEnd ();
				r.Close ();
				_data = _info;
			}
			
			if (!string.IsNullOrEmpty (_data) && doLog)
			{
				ACDebug.Log ("File Read: " + fullFilename);
			}
			return (_data);
		}

		#endregion


		#region PrivateFunctions

		private void OnGatherFilesForDeletion (List<SaveFile> saveFiles)
		{
			foreach (SaveFile saveFile in saveFiles)
			{
				Delete (saveFile);
			}
		}

		#endregion


		#region GetSet

		protected virtual int MaxSaves { get { return 50; } }

		#endregion

	}

	#endif

}