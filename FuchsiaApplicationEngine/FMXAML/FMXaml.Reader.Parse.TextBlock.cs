﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Controls;

namespace Fuchsia.InformationEngine
{
    internal enum FMXamlTextBlockAttributes { Content, FontStyle, FontSize, FontWeight, FontFamily, FontColour } // preparation
    public partial class FMXamlReader
    {
        
        internal RichTextBox FMXAML_Parse_TextBlock(XmlNode NodeToParse, RichTextBox BoxToPopulate)
        {
            XmlAttributeCollection FXmlNodeAttributes = NodeToParse.Attributes; // should be verified by now.

            foreach (XmlAttribute FXmlAttribute in FXmlNodeAttributes)
            {
                switch (FXmlAttribute.Name) // TEMP, Pre-Factory Class code.
                {
                    case "content":
                    case "Content":
                        try
                        {  //TEMPCODE
                            BoxToPopulate = FMXAML_TextAPI_AddText(BoxToPopulate, FXmlAttribute.Value, 18);
                        }
                        catch (NotImplementedException)
                        {
                            FError.ThrowError(12, $"API call not implemented", FErrorSeverity.FatalError);
                        }
                        continue;
                    case "fontstyle":
                    case "FontStyle":
                        continue; 
                }
            }

            return BoxToPopulate;
        }
    }
}