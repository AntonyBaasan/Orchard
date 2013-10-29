﻿using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Navigation.Models {
    public class MenuWidgetPart : ContentPart<MenuWidgetPartRecord> {

        public int StartLevel {
            get { return int.Parse(Get("StartLevel") ?? "0", CultureInfo.InvariantCulture); }
            set {
                Set("StartLevel", value.ToString(CultureInfo.InvariantCulture));
                Record.StartLevel = value;
            }
        }

        public int Levels {
            get { return int.Parse(Get("Levels") ?? "0", CultureInfo.InvariantCulture); }
            set {
                Set("Levels", value.ToString(CultureInfo.InvariantCulture));
                Record.Levels = value;
            }
        }

        public bool Breadcrumb {
            get { return bool.Parse(Get("Breadcrumb") ?? "false"); }
            set {
                Set("Breadcrumb", value.ToString());
                Record.Breadcrumb = value;
            }
        }

        public bool AddHomePage {
            get { return bool.Parse(Get("AddHomePage") ?? "false"); }
            set {
                Set("AddHomePage", value.ToString());
                Record.AddHomePage = value;
            }
        }

        public bool AddCurrentPage {
            get { return bool.Parse(Get("AddCurrentPage") ?? "false"); }
            set {
                Set("AddCurrentPage", value.ToString());
                Record.AddCurrentPage = value;
            }
        }
        
        public ContentItemRecord Menu {
            get { return Record.Menu; }
            set { Record.Menu = value; }
        }
    }
}