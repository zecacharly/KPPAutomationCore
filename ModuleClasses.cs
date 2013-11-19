using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;
using System.Xml.Serialization;
using KPP.Core.Debug;
using IOModule;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using DejaVu;
using VisionModule;
using EpsonModule;
//using VisionModule;

namespace KPPAutomationCore {

    public delegate void SelectedProjectChanged(ModuleProject ProjectSelected);

    [XmlInclude(typeof(VisionProjects))]
    [XmlInclude(typeof(EpsonProjects))]
    public class ModuleProjects {

        
        #region -  Serialization attributes  -

        internal static Int32 S_BackupFilesToKeep = 5;
        internal static String S_BackupFolderName = "backup";
        internal static String S_BackupExtention = "bkp";
        internal static String S_DefaulFileExtention = "xml";

        private String _filePath = null;
        private String _defaultPath = null;

        [XmlIgnore]
        public Int32 BackupFilesToKeep { get; set; }
        [XmlIgnore]
        public String BackupFolderName { get; set; }
        [XmlIgnore]
        public String BackupExtention { get; set; }

        #endregion

        private static KPPLogger log = new KPPLogger(typeof(ModuleProjects));

        [XmlAttribute]
        public virtual String Name { get; set; }

        public string ModuleName {
            get;
            set;
        }



        public virtual List<ModuleProject> Projects { get; set; }

        

        /// <summary>
        /// 
        /// </summary>
        public ModuleProjects() {
            Name = "Vision Projects";
            Projects = new List<ModuleProject>();
        }


        //    StaticObjects.ListInspections.Add(item);

        #region Read Operations

        /// <summary>
        /// Reads the configuration.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        internal static ModuleProjects ReadConfigurationFile(string path) {
            //log.Debug(String.Format("Load Xml file://{0}", path));
            if (File.Exists(path)) {
                ModuleProjects result = null;
                TextReader reader = null;

                try {
                    XmlSerializer serializer = new XmlSerializer(typeof(ModuleProjects));
                    reader = new StreamReader(path);

                    UndoRedoManager.StartInvisible("Init");

                    ModuleProjects config = serializer.Deserialize(reader) as ModuleProjects;

                    config._filePath = path;

                    result = config;
                    UndoRedoManager.Commit();
                } catch (Exception exp) {
                    log.Error(exp);
                } finally {
                    if (reader != null) {
                        reader.Close();
                    }
                }
                return result;
            }
            return null;
        }

        /// <summary>
        /// Reads the configuration.
        /// </summary>
        /// <param name="childtype">The childtype.</param>
        /// <param name="xmlString">The XML string.</param>
        /// <returns></returns>
        internal static ModuleProjects ReadConfigurationString(string xmlString) {
            try {
                XmlSerializer serializer = new XmlSerializer(typeof(ModuleProjects));
                ModuleProjects config = serializer.Deserialize(new StringReader(xmlString)) as ModuleProjects;

                return config;
            } catch (Exception exp) {
                log.Error(exp);
            }
            return null;
        }

        #endregion

        #region Write Operations

        /// <summary>
        /// Writes the configuration.
        /// </summary>
        public void WriteConfigurationFile() {
            if (_filePath != null) {

                WriteConfigurationFile(_filePath);

            }
        }

        /// <summary>
        /// Writes the configuration.
        /// </summary>
        /// <param name="path">The path.</param>
        public void WriteConfigurationFile(string path) {

            WriteConfiguration(this, path, BackupFolderName, BackupExtention, BackupFilesToKeep);

        }

        /// <summary>
        /// Writes the configuration string.
        /// </summary>
        /// <returns></returns>
        public String WriteConfigurationToString() {

            return WriteConfigurationToString(this);
        }

        /// <summary>
        /// Writes the configuration.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="path">The path.</param>
        internal static void WriteConfiguration(ModuleProjects config, string path) {
            WriteConfiguration(config, path, S_BackupFolderName, S_BackupExtention, S_BackupFilesToKeep);
        }

        /// <summary>
        /// Writes the configuration.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="path">The path.</param>
        internal static void WriteConfiguration(ModuleProjects config, string path, string backupFolderName, String backupExtention, Int32 backupFilesToKeep) {
            if (File.Exists(path) && backupFilesToKeep > 0) {
                //Do a file backup prior to overwrite
                try {
                    //Check if valid backup folder name
                    if (backupFolderName == null || backupFolderName.Length == 0) {
                        backupFolderName = "backup";
                    }

                    //Check Backup folder
                    String bkpFolder = Path.Combine(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Config"), backupFolderName);
                    if (!Directory.Exists(bkpFolder)) {
                        Directory.CreateDirectory(bkpFolder);
                    }

                    //Check extention
                    String ext = backupExtention != null && backupExtention.Length > 0 ? backupExtention : Path.GetExtension(path);
                    if (!ext.StartsWith(".")) { ext = String.Format(".{0}", ext); }

                    //Delete existing backup file (This should not exist)
                    String bkpFile = Path.Combine(bkpFolder, String.Format("{0}_{1:yyyyMMddHHmmss}{2}", Path.GetFileNameWithoutExtension(path), DateTime.Now, ext));
                    if (File.Exists(bkpFile)) { File.Delete(bkpFile); }

                    //Delete excess backup files
                    String fileSearchPattern = String.Format("{0}_*{1}", Path.GetFileNameWithoutExtension(path), ext);
                    String[] bkpFilesList = Directory.GetFiles(bkpFolder, fileSearchPattern, SearchOption.TopDirectoryOnly);
                    if (bkpFilesList != null && bkpFilesList.Length > (backupFilesToKeep - 1)) {
                        bkpFilesList = bkpFilesList.OrderByDescending(f => f.ToString()).ToArray();
                        for (int i = (backupFilesToKeep - 1); i < bkpFilesList.Length; i++) {
                            File.Delete(bkpFilesList[i]);
                        }
                    }

                    //Backup current file
                    File.Copy(path, bkpFile);
                    //log.Debug(String.Format("Backup file://{0} to file://{1}", path, bkpFile));
                } catch (Exception exp) {
                    //log.Error(String.Format("Error copying file {0} to backup.", path), exp);
                }
            }
            try {

                XmlSerializer serializer = new XmlSerializer(config.GetType());
                TextWriter textWriter = new StreamWriter(path);
                serializer.Serialize(textWriter, config);
                textWriter.Close();

                //log.Debug(String.Format("Write Xml file://{0}", path));
            } catch (Exception exp) {
                log.Error("Error writing configuration. ", exp);

                Console.WriteLine(exp.ToString());
            }
        }

        /// <summary>
        /// Writes the configuration to string.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        internal static String WriteConfigurationToString(ModuleProjects config) {
            try {
                XmlSerializer serializer = new XmlSerializer(config.GetType());
                StringWriter stOut = new StringWriter();

                serializer.Serialize(stOut, config);

                return stOut.ToString();
            } catch (Exception exp) {
                //log.Error("Error writing configuration. ", exp);

            }
            return null;
        }

        #endregion

    }

    [XmlInclude(typeof(VisionProject))]
    [XmlInclude(typeof(EpsonProject))]
    public class ModuleProject : ICloneable {


        private KPPLogger _log;
        [XmlIgnore,Browsable(false)]
        public virtual KPPLogger log {
            get { return _log; }
            set { _log = value; }
        }



        
        [XmlAttribute, Browsable(false)]
        public virtual String ModuleName {
            get;
            set;
        }

        [XmlAttribute]
        public virtual String Name { get; set; }

        [XmlAttribute("ID")]
        public virtual int ProjectID { get; set; }


        private bool _loadonstart = false;
        [XmlAttribute]
        public virtual bool Loadonstart {
            get { return _loadonstart; }
            set {
                _loadonstart = value;
            }
        }

        public virtual object Clone() {
            return this.MemberwiseClone();
        }

        public virtual void Dispose() {

        }
    }

    public class ModuleSettings {


        public event SelectedProjectChanged OnSelectedProjectChanged;


        #region -  Serialization attributes  -

        public static Int32 S_BackupFilesToKeep = 5;
        public static String S_BackupFolderName = "backup";
        public static String S_BackupExtention = "bkp";
        public static String S_DefaulFileExtention = "xml";

        
        private String _FilePath = null;
        [XmlIgnore, Browsable(false)]
        public virtual String FilePath {
            get { return _FilePath; }
            set { _FilePath = value; }
        }

        private String _defaultPath = null;

        [XmlIgnore]
        public Int32 BackupFilesToKeep { get; set; }
        [XmlIgnore]
        public String BackupFolderName { get; set; }
        [XmlIgnore]
        public String BackupExtention { get; set; }

        #endregion
                

        [XmlAttribute]
        public virtual String Name { get; set; }


        public virtual List<String> ProjectDirectories { get; set; }

        public virtual List<TCPServer> Servers { get; set; }

        [XmlAttribute]
        public virtual String ProjectFileExtension { get; set; }

        [XmlAttribute]
        public virtual String ProjectFile { get; set; }

        private String m_ModuleName;
        [XmlAttribute]
        public virtual String ModuleName {
            get { return m_ModuleName; }
            set { m_ModuleName = value; }
        }


        private ModuleProject _SelectedProject = null;
        [XmlIgnore,Browsable(false)]
        public virtual ModuleProject SelectedProject {
            get { return _SelectedProject; }
            set {
                _SelectedProject = value;
                if (OnSelectedProjectChanged != null) {
                    OnSelectedProjectChanged(value);
                }

            }
        }
     
        private string m_DockFile;// = Path.Combine(FilesLocation, ModuleName + "DockPanel.dock");
        [XmlIgnore, Browsable(false)]
        public virtual string DockFile {
            get { return m_DockFile; }
            set { m_DockFile = value; }
        }
        




        /// <summary>
        /// 
        /// </summary>
        public ModuleSettings() {
           
        }

    }

    public interface IModuleForm {

        void InitModule(String moduleName, String visionSettingsFile);


        
        Boolean Restart {
            get;
            set;
        }
        //event SelectedProjectChanged OnSelectedProjectChanged;
        
    }
}
