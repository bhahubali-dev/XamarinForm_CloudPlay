using System;
using System.ComponentModel;
using Android.Graphics.Drawables;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Switch = Android.Widget.Switch;

namespace PlayOnCloud.Droid.Renderers
{
    public class CustomSwitchRenderer : ViewRenderer<CustomSwitch, Switch>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<CustomSwitch> e)
        {
            if (Control == null)
            {
                var toggle = new Switch(Context);
                toggle.CheckedChange += ControlValueChanged;
                SetNativeControl(toggle);
            }

            base.OnElementChanged(e);

            if (e.OldElement != null)
                e.OldElement.Toggled -= ElementToggled;

            if (e.NewElement != null)
            {
                Control.Checked = e.NewElement.IsToggled;
                Element.Toggled += ElementToggled;
            }
        }


        /// <summary>
        ///     Handles the <see cref="E:ElementPropertyChanged" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            //if (e.PropertyName == "TintColor")
            //{
            //    //this.SetTintColor(this.Element.TintColor);
            //}
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Control.CheckedChange -= ControlValueChanged;
                Element.Toggled -= ElementToggled;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Sets the color of the tint.
        /// </summary>
        /// <param name="color">The color.</param>
        private void SetTintColor(Color color)
        {
            var thumbStates = new StateListDrawable();
            thumbStates.AddState(new[] {Android.Resource.Attribute.StateChecked}, new ColorDrawable(color.ToAndroid()));
            //thumbStates.AddState(new int[]{-android.R.attr.state_enabled}, new ColorDrawable(colorDisabled));
            //thumbStates.addState(new int[]{}, new ColorDrawable(this.app.colorOff)); // this one has to come last
            Control.ThumbDrawable = thumbStates;
        }

        /// <summary>
        ///     Handles the Toggled event of the Element control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ToggledEventArgs" /> instance containing the event data.</param>
        private void ElementToggled(object sender, ToggledEventArgs e)
        {
            Control.Checked = Element.IsToggled;
        }

        /// <summary>
        ///     Handles the ValueChanged event of the Control control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ControlValueChanged(object sender, EventArgs e)
        {
            Element.IsToggled = Control.Checked;
        }
    }
}