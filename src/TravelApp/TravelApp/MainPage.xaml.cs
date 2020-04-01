using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TravelApp
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        SKPaint backgroundPaint = new SKPaint()
        {
            Color = SKColors.White,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        private double _density;

        float _cutoutPosPx = 0;
        float _cutoutHeightPx;
        private double _cutoutPos;
        float _cutoutHeight = 300;
        float _padding = 15;

        enum State
        {
            Collapsed,
            Expanded
        }

        State _currentState = State.Collapsed;
        private float _expandValuePx;

        public MainPage()
        {
            InitializeComponent();
            _density = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;
            _padding = 15 * (float)_density;
        }



        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            // how big should my cutout be
            _cutoutHeightPx = (float)(_cutoutHeight * _density);
            _cutoutPos = height - (_cutoutHeight + _padding);
            _cutoutPosPx = (float)(_cutoutPos * _density);
            Video.TranslationY = _cutoutPos;
        }



        private void SkiaOverlay_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            float left = _padding;
            float top = _cutoutPosPx;
            float right = info.Width - _padding;
            float bottom = top + _cutoutHeightPx;

            // adjuist the cutout based on the expand animation
            if (_expandValuePx > 0)
            {
                // change the values
                left -= _expandValuePx;
                if (left < 0) left = 0;

                right += _expandValuePx;
                if (right > info.Width) right = info.Width;

                top -= _expandValuePx;
                if (top < 0) top = 0;

                bottom += _expandValuePx;
                if (bottom > info.Height) bottom = info.Height;
            }

            System.Diagnostics.Debug.WriteLine($"Rect Bounds {left}, {top}, {right}, {bottom}");

            // create a cutout
            var cutoutRect = new SKRect(left, top, right, bottom);
            canvas.ClipRoundRect(new SKRoundRect(cutoutRect, _padding, _padding),
                SKClipOperation.Difference, true);


            // draw the background
            canvas.DrawRect(new SKRect(0, 0, info.Width, info.Height), backgroundPaint);

        }


        private void TouchEff_Completed(VisualElement sender, TouchEffect.EventArgs.TouchCompletedEventArgs args)
        {
            _currentState = _currentState == State.Collapsed ? State.Expanded : State.Collapsed;
            GotoState(_currentState);

        }

        private void GotoState(State currentState)
        {
            double startPos = 0;
            double endPos = 0;
            double startExpand = 0;
            double endExpand = 0;
            

            if (currentState == State.Collapsed)
            {
                startPos = 0;
                //endPos = this.Height - ((_cutoutHeightPx + _padding) / _density);
                endPos = this.Height - (_cutoutHeight + _padding);

                startExpand = this.Height;
                endExpand = 0;
            }
            else
            {
                endPos = 0;
                startPos = this.Height - (_cutoutHeight + _padding);

                startExpand = 0;
                endExpand = this.Height;
            }

            // set our initial value
            _cutoutPosPx = (float)startPos * (float)_density;
            _expandValuePx = (float)startExpand * (float)(_density);

            // animate cutoutPos
            Animation cutoutAnim = new Animation(t =>
            {
                // adjust the position of the cutout
                _cutoutPosPx = (float)t * (float)_density;
                // adjust position of video to match
                Video.TranslationY = t;
                // invalidate the canvas
                SkiaOverlay.InvalidateSurface();

            }, startPos, endPos, Easing.SinInOut);


            Animation expandAnim = new Animation(exp =>
            {
                // adjust the position of the cutout
                _expandValuePx = (float)exp * (float)_density;
                // invalidate the canvas
                SkiaOverlay.InvalidateSurface();

            }, startExpand, endExpand, Easing.SinInOut);

            Animation parentAnimation = new Animation();

            if (currentState == State.Expanded)
            {
                parentAnimation.Add(0.00, 0.50, cutoutAnim);
                parentAnimation.Add(0.50, 1.00, expandAnim);
            }
            else
            {
                parentAnimation.Add(0.0, 0.50, expandAnim);
                parentAnimation.Add(0.50, 1.00, cutoutAnim);
            }

            parentAnimation.Commit(this, "ExpandAnimation", 16, 2000);
        }
    }
}
