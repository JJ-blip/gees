﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LsideWPF.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.5.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AutoCloseLanding {
            get {
                return ((bool)(this["AutoCloseLanding"]));
            }
            set {
                this["AutoCloseLanding"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public int CloseAfterLanding {
            get {
                return ((int)(this["CloseAfterLanding"]));
            }
            set {
                this["CloseAfterLanding"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("30")]
        public int MaxTaxiSpeedKts {
            get {
                return ((int)(this["MaxTaxiSpeedKts"]));
            }
            set {
                this["MaxTaxiSpeedKts"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("100")]
        public int LandingThresholdFt {
            get {
                return ((int)(this["LandingThresholdFt"]));
            }
            set {
                this["LandingThresholdFt"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("MSFS2020Landings-Lside")]
        public string LandingDirectory {
            get {
                return ((string)(this["LandingDirectory"]));
            }
            set {
                this["LandingDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Landing.v5.csv")]
        public string LandingFile {
            get {
                return ((string)(this["LandingFile"]));
            }
            set {
                this["LandingFile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool enableTouchAndGo {
            get {
                return ((bool)(this["enableTouchAndGo"]));
            }
            set {
                this["enableTouchAndGo"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1000")]
        public int SlipLoggingThresholdFt {
            get {
                return ((int)(this["SlipLoggingThresholdFt"]));
            }
            set {
                this["SlipLoggingThresholdFt"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SlipLoggingIsEnabled {
            get {
                return ((bool)(this["SlipLoggingIsEnabled"]));
            }
            set {
                this["SlipLoggingIsEnabled"] = value;
            }
        }
    }
}
