using System.Windows.Forms;

using GH_IO.Serialization;

using Grasshopper.Kernel;

namespace D2P.GHPlugin.Components.GHComponents {
    /// <summary>
    /// Base class for components that stream data from Rhino and can become
    /// outdated when the Rhino document changes.
    /// </summary>
    public abstract class GHComponentStreamBase : GHVariableParameterComponent {
        const string AutoUpdateKey = "AutoUpdate";
        const string OutdatedMessage = "OUTDATED";
        const string OutdatedWarning = "Stream data is outdated. Press Reload or enable AutoReload in the component menu.";

        bool _isOutdated;
        bool _autoUpdate = false;

        public bool IsOutdated => _isOutdated;

        protected GHComponentStreamBase(
            string name,
            string shortname,
            string description,
            string category,
            string subcategory)
            : base(name,shortname,description,category,subcategory) { }

        public override void CreateAttributes() {
            m_attributes = new GHComponentStreamAttributes(this);
        }

        /// <summary>
        /// Triggered by the on-component Reload button.
        /// </summary>
        public void RequestReload() {
            ExpireSolution(true);
        }

        protected void ClearOutdated() {
            if (!_isOutdated && string.IsNullOrEmpty(Message))
                return;

            _isOutdated = false;
            if (Message == OutdatedMessage)
                Message = string.Empty;

            Attributes?.ExpireLayout();
            OnDisplayExpired(true);
        }

        public void MarkStreamOutdated() {
            _isOutdated = true;

            if (_autoUpdate) {
                ScheduleReload();
                return;
            }

            Message = OutdatedMessage;
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,OutdatedWarning);
            Attributes?.ExpireLayout();
            OnDisplayExpired(true);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) {
            base.AppendAdditionalComponentMenuItems(menu);
            var menuItem = Menu_AppendItem(menu,"AutoReload",ToggleAutoUpdate,true,_autoUpdate);
            menuItem.ToolTipText = "When enabled, automatically reloads from Rhino whenever the stream becomes outdated";
        }

        public override bool Write(GH_IWriter writer) {
            writer.SetBoolean(AutoUpdateKey,_autoUpdate);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader) {
            if (reader.ItemExists(AutoUpdateKey))
                _autoUpdate = reader.GetBoolean(AutoUpdateKey);
            return base.Read(reader);
        }

        void ToggleAutoUpdate(object sender,System.EventArgs e) {
            _autoUpdate = !_autoUpdate;
            OnObjectChanged(GH_ObjectEventType.Options);

            if (_autoUpdate && _isOutdated)
                ScheduleReload();

            Attributes?.ExpireLayout();
            OnDisplayExpired(true);
        }

        void ScheduleReload() {
            var ghDoc = OnPingDocument();
            if (ghDoc != null)
                ghDoc.ScheduleSolution(5,d => ExpireSolution(false));
            else
                ExpireSolution(true);
        }
    }
}
