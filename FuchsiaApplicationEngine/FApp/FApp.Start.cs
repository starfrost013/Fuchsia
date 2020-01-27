﻿using System.IO;
using System.Linq;
using System.Xml;

/// <summary>
/// 
/// Fuchsia Information Engine
/// 
/// </summary>

namespace Fuchsia.InformationEngine
{
    public partial class FInfoEngine
    {
        public IFApp FStartApp(string PathToApp, string AppName) // "Compiled FApps" coming soon.
        {
            try
            {
                IFApp FApp = new FApp();

                FApp.AppPage = new XmlDocument();
                string _burn = $"{PathToApp}\\{AppName}\\App.xml";

                if (_burn.Contains(':'))
                {
                    _burn.Remove(':');
                }

                FApp.AppPage.Load(_burn);
                FApp.AppPath = PathToApp;
                FApp = FStartApp_ParseAppXml(FApp, FApp.AppPage);

                FApp.TitlePage = FStartApp_LoadTitlePage(FApp);

                return FApp;
            }
            catch (XmlException err)
            {
                FError.ThrowError(0, $"An error has occurred loading the app definition page located at {PathToApp}. Aborting load...", FErrorSeverity.Error, err);
                return null;
            }
        }

        /// <summary>
        /// Internal API for loading XML.
        /// </summary>
        /// <param name="path">The path to the XML file.</param>
        /// <returns></returns>
        internal XmlDocument FLoadXml(string Path)
        {
            try
            {
                XmlDocument FXmlLoad = new XmlDocument();

                if (Path.Contains(':'))
                {
                    Path.Remove(':');
                }

                FXmlLoad.Load(Path);
                return FXmlLoad;
            }
            catch (XmlException err)
            {
                FError.ThrowError(8, $"An error has occurred loading XML file {Path}. Aborting load...", FErrorSeverity.Error, err);
                return null;
            }
        }

        /// <summary>
        /// Internal API for loading XML.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="VerifyNodeName"></param>
        /// <returns></returns>
        internal XmlDocument FLoadXml(string Path, string VerifyNodeName = null)
        {
            try
            {
                XmlDocument FXmlLoad = new XmlDocument();
                FXmlLoad.Load(Path);

                if (FXmlLoad.FirstChild.Name != VerifyNodeName)
                {
                    FError.ThrowError(10, $"XML file {Path} does not have {VerifyNodeName} as its root node. Aborting load...", FErrorSeverity.Error);
                }

                return FXmlLoad;
            }
            catch (XmlException err)
            {
                FError.ThrowError(9, $"An error has occurred loading XML file {Path}. Aborting load...", FErrorSeverity.Error, err);
                return null;
            }
        }

        /// <summary>
        /// Internal API for parsing the App Definition XML.
        /// </summary>
        /// <param name="FApp">The IFApp to load.</param>
        /// <param name="FAppXml">The XML file to load the IFApp attributes from.</param>
        /// <returns></returns>
        /// 
        internal IFApp FStartApp_ParseAppXml(IFApp FApp, XmlDocument FAppXml)
        {
            try
            {
                XmlNode FXmlRootNode = FAppXml.FirstChild;

                if (FXmlRootNode.Name != "Fuchsia")
                {
                    FError.ThrowError(1, $"App App.xml does not have Fuchsia as its first node. Aborting load...", FErrorSeverity.Error);
                    return null;
                }

                XmlNodeList FXmlFirstLevelNodes = FXmlRootNode.ChildNodes;

                foreach (XmlNode FXmlFirstLevelNode in FXmlFirstLevelNodes)
                {
                    switch (FXmlFirstLevelNode.Name)
                    {
                        case "AppInformation":
                            if (FXmlFirstLevelNode.Attributes.Count == 0) FError.ThrowError(4, $"App definition empty. Aborting load...", FErrorSeverity.Error);
                            XmlAttributeCollection FXmlAppInfoAttributes = FXmlFirstLevelNode.Attributes;

                            foreach (XmlAttribute FXmlAppInfoAttribute in FXmlAppInfoAttributes)
                            {
                                switch (FXmlAppInfoAttribute.Name)
                                {
                                    case "author":
                                        FApp.AppAuthor = FXmlAppInfoAttribute.Value;
                                        continue;
                                    case "description":
                                        FApp.AppDescription = FXmlAppInfoAttribute.Value;
                                        continue;
                                    case "developer":
                                        FApp.AppDeveloper = FXmlAppInfoAttribute.Value;
                                        continue;
                                    case "name":
                                        FApp.AppName = FXmlAppInfoAttribute.Value;
                                        continue;
                                    case "purpose":
                                        FApp.AppPurpose = FXmlAppInfoAttribute.Value;
                                        continue;
                                }

                                if (FApp.AppName == null) FError.ThrowError(5, $"Attempted to load app with no name. Aborting load...", FErrorSeverity.Error);
                            }

                            continue;
                        default:
                            FError.ThrowError(3, $"Invalid node name present in App.xml, aborting load...", FErrorSeverity.Error);
                            return null;
                    }
                }

                return FApp;
            }
            catch (XmlException err)
            {
                FError.ThrowError(2, $"App App.xml corrupted. Aborting load...", FErrorSeverity.Error, err);
                return null;
            }
            catch (FileNotFoundException err)
            {
                FError.ThrowError(6, $"Attempted to load a nonexistent App.xml.", FErrorSeverity.Error, err);
                return null;
            }
            catch (DirectoryNotFoundException err)
            {
                FError.ThrowError(7, $"Attempted to load a nonexistent App.xml.", FErrorSeverity.Error, err);
                return null;
            }
        }

        /// <summary>
        /// Internal API for loading title pages of Apps.
        /// </summary>
        /// <param name="AppToLoadTitlePageOf">The </param>
        /// <returns>An FDocumentTitlePage containing the loaded app title page.</returns>
        internal FDocumentTitlePage FStartApp_LoadTitlePage(IFApp AppToLoadTitlePageOf)
        {
            FDocumentTitlePage FDocumentTitlePage = new FDocumentTitlePage();
            FDocumentTitlePage.DocumentXML = FLoadXml($"{AppToLoadTitlePageOf.AppPath}\\{AppToLoadTitlePageOf.AppName}\\Title.xml", "Fuchsia");
            if (!FStartApp_VerifyAttributes(FDocumentTitlePage))
            {
                return null;
            }
            else
            {
                FStartApp_GetDocumentMetadataXml(FDocumentTitlePage);
            }

            FDocumentTitlePage.DocumentRTB = FParseDocument(FDocumentTitlePage);
            FuchsiaHome FuchsiaHome = new FuchsiaHome(FDocumentTitlePage.DocumentRTB); //tempcode.
            FuchsiaHome.Show(); 
            return FDocumentTitlePage;
        }

        /// <summary>
        /// Internal API for verifying that an XML node has attributes. Throws error 11 and returns false if failed.
        /// </summary>
        /// <param name="DocumentToVerify">The FDocument to verify</param>
        /// <returns>true if the verification was successful, throws error 11 and returns false otherwise.</returns>
        internal bool FStartApp_VerifyAttributes(IFDocument DocumentToVerify)
        {
            if (DocumentToVerify.DocumentXML.FirstChild.Attributes.Count == 0)
            {
                FError.ThrowError(11, $"The node {DocumentToVerify.DocumentXML.FirstChild.Name} in document {DocumentToVerify.DocumentPath} must have attributes.", FErrorSeverity.Error);
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Internal API for verifying that an XML node has child nodes. Throws error 13 and returns false if failed.
        /// </summary>
        /// <param name="DocumentToVerify">The FDocument to verify</param>
        /// <returns>true if the verification was successful, throws error 13 and returns false otherwise.</returns>
        internal bool FStartApp_VerifyChildNodes(IFDocument DocumentToVerify)//public variant too
        {
            if (!DocumentToVerify.DocumentXML.FirstChild.HasChildNodes)
            {
                FError.ThrowError(13, $"The node {DocumentToVerify.DocumentXML.FirstChild.Name} in document {DocumentToVerify.DocumentPath} must have child nodes.", FErrorSeverity.Error);
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool FGetDocumentMetadata(IFDocument DocumentToGetMetadata)
        {
            FError.ThrowError(12, $"API call not implemented", FErrorSeverity.FatalError);
            return false;
        }

        /// <summary>
        /// Internal API for getting document metadata from XML
        /// </summary>
        /// <param name="DocumentToGetMetadata">The Document to get the metadata from</param>
        /// <returns>The document, now with metadata.</returns>
        internal IFDocument FStartApp_GetDocumentMetadataXml(IFDocument DocumentToGetMetadata)
        {
            XmlNode FDocumentRootXMLNode = DocumentToGetMetadata.DocumentXML.FirstChild;

            // FLoadXml has already verified that the first node is Fuchsia and FStartApp_VerifyAttributes has verified that there are attributes, so no error checking required


            XmlAttributeCollection FDocumentXMLFirstLevelNodes = FDocumentRootXMLNode.Attributes;

            foreach (XmlNode FDocumentXMLNode in FDocumentXMLFirstLevelNodes)
            {
                switch (FDocumentXMLNode.Name)
                {
                    case "description":
                        DocumentToGetMetadata.DocumentDescription = FDocumentXMLNode.Value;
                        continue;
                    case "name":
                        DocumentToGetMetadata.DocumentName = FDocumentXMLNode.Value;
                        continue;
                    case "title":
                        DocumentToGetMetadata.DocumentTitle = FDocumentXMLNode.Value;
                        continue;
                }
            }

            return DocumentToGetMetadata;
            
        } 
    }
}