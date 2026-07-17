using System.Drawing;
using System.Windows.Forms;

using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;

namespace D2P.GHPlugin.Components.GHComponents {
    public class GHComponentStreamAttributes : GH_ComponentAttributes {
        const int ButtonHeight = 22;
        const int ButtonGap = 3;
        const int ButtonInset = 2;

        RectangleF _buttonBounds;
        bool _buttonCaptured;
        bool _buttonPressed;

        public GHComponentStreamAttributes(GHComponentStreamBase owner)
            : base(owner) { }

        protected override void Layout() {
            base.Layout();

            var bounds = Bounds;
            bounds.Height += ButtonGap + ButtonHeight;

            _buttonBounds = new RectangleF(
                bounds.X + ButtonInset,
                bounds.Bottom - ButtonHeight - ButtonInset,
                bounds.Width - 2 * ButtonInset,
                ButtonHeight);

            Bounds = bounds;
        }

        protected override void Render(GH_Canvas canvas,Graphics graphics,GH_CanvasChannel channel) {
            base.Render(canvas,graphics,channel);

            if (channel != GH_CanvasChannel.Objects)
                return;

            var streamOwner = Owner as GHComponentStreamBase;
            var palette = GH_Palette.Black;
            var renderBounds = _buttonBounds;

            if (_buttonPressed) {
                renderBounds.Inflate(-1,-1);
            }
            else {
                var shadowBounds = _buttonBounds;
                shadowBounds.Inflate(-1,-1);
                using (var shadow = new SolidBrush(Color.FromArgb(80,Color.Black)))
                    graphics.FillRectangle(shadow,shadowBounds);
            }

            using (var capsule = GH_Capsule.CreateTextCapsule(
                renderBounds,
                renderBounds,
                palette,
                "Reload",
                2,
                0)) {
                capsule.Render(graphics,Selected,Owner.Locked,false);
            }
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender,GH_CanvasMouseEvent e) {
            if (e.Button == MouseButtons.Left && _buttonBounds.Contains(e.CanvasLocation)) {
                var streamOwner = Owner as GHComponentStreamBase;
                if (streamOwner != null && !streamOwner.Locked) {
                    _buttonCaptured = true;
                    _buttonPressed = true;
                    sender.Invalidate();
                    return GH_ObjectResponse.Capture;
                }
            }

            return base.RespondToMouseDown(sender,e);
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender,GH_CanvasMouseEvent e) {
            if (_buttonCaptured) {
                var pressed = _buttonBounds.Contains(e.CanvasLocation);
                if (_buttonPressed != pressed) {
                    _buttonPressed = pressed;
                    sender.Invalidate();
                }
                return GH_ObjectResponse.Handled;
            }

            return base.RespondToMouseMove(sender,e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender,GH_CanvasMouseEvent e) {
            if (_buttonCaptured) {
                var reload = _buttonPressed && e.Button == MouseButtons.Left;
                _buttonCaptured = false;
                _buttonPressed = false;
                sender.Invalidate();

                if (reload) {
                    var streamOwner = Owner as GHComponentStreamBase;
                    if (streamOwner != null && !streamOwner.Locked)
                        streamOwner.RequestReload();
                }

                return GH_ObjectResponse.Release;
            }

            return base.RespondToMouseUp(sender,e);
        }
    }
}
