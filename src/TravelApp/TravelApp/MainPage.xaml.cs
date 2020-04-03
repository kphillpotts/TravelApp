using SkiaSharp;
using SkiaSharp.Views.Forms;
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
        SKPaint flyupPaint = new SKPaint()
        {
            Color = SKColors.White,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        SKPaint centerCirclePaint = new SKPaint()
        {
            Color = Color.FromHex("1E2221").ToSKColor(),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };


        private double _density;
        private SKBitmap mapbackground;

        // constant values
        const double padding = 15;
        const double cellHeight = 300;
        State _currentState = State.Collapsed;

        // holds our animation values for states and current
        private AnimationHelper anim = new AnimationHelper();

        public MainPage()
        {
            InitializeComponent();
            _density = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;
            mapbackground = BitmapExtensions.LoadBitmapResource(this.GetType(), "TravelApp.Images.backgroundmap.png");
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            SetupAnimations();
            Video.TranslationY = anim.GetStateValue(AnimationKey.Cell, State.Collapsed);
        }

        private void SetupAnimations()
        {
            anim.SetStateValue(AnimationKey.Cell, State.Collapsed, this.Height - (cellHeight + padding));
            anim.SetStateValue(AnimationKey.Cell, State.Expanded, 0);
            anim[AnimationKey.Cell].Current = anim.GetStateValue(AnimationKey.Cell, State.Collapsed);

            anim.SetStateValue(AnimationKey.Expand, State.Collapsed, 0);
            anim.SetStateValue(AnimationKey.Expand, State.Expanded, this.Height);
            anim[AnimationKey.Expand].Current = anim.GetStateValue(AnimationKey.Expand, State.Collapsed);

            anim.SetStateValue(AnimationKey.FlyUp, State.Collapsed, this.Height);
            anim.SetStateValue(AnimationKey.FlyUp, State.Expanded, this.Height - 300);
            anim[AnimationKey.FlyUp].Current = anim.GetStateValue(AnimationKey.FlyUp, State.Collapsed);

            anim.SetStateValue(AnimationKey.Circle, State.Collapsed, 0);
            anim.SetStateValue(AnimationKey.Circle, State.Expanded, 50);
            anim[AnimationKey.Circle].Current = anim.GetStateValue(AnimationKey.Circle, State.Collapsed);
        }


        private void TouchEff_Completed(VisualElement sender, TouchEffect.EventArgs.TouchCompletedEventArgs args)
        {
            _currentState = _currentState == State.Collapsed ? State.Expanded : State.Collapsed;
            GotoState(_currentState);
        }

        private void GotoState(State currentState)
        {
            // animate cutoutPos
            Animation cutoutAnim = new Animation(
                callback: t =>
                {
                    // adjust the position of the cutout
                    anim[AnimationKey.Cell].Current = t;
                    Video.TranslationY = t;
                    
                    SkiaOverlay.InvalidateSurface();

                },
                start: anim[AnimationKey.Cell].Current,
                end: anim.GetStateValue(AnimationKey.Cell, currentState),
                easing: Easing.SinInOut);


            Animation expandAnim = new Animation(
                callback: exp =>
                {
                    anim[AnimationKey.Expand].Current = exp;
                    SkiaOverlay.InvalidateSurface();

                },
                start: anim[AnimationKey.Expand].Current,
                end: anim.GetStateValue(AnimationKey.Expand, currentState),
                easing: Easing.SinInOut);

            Animation flyupAnim = new Animation(
                callback: flyup =>
                {
                    anim[AnimationKey.FlyUp].Current = flyup;
                    SkiaOverlay.InvalidateSurface();

                },
                start: anim[AnimationKey.FlyUp].Current,
                end: anim.GetStateValue(AnimationKey.FlyUp, currentState),
                easing: Easing.SinInOut);

            Animation circleAnim = new Animation(
                callback: circle =>
                {
                    anim[AnimationKey.Circle].Current = circle;
                    SkiaOverlay.InvalidateSurface();

                },
                start: anim[AnimationKey.Circle].Current,
                end: anim.GetStateValue(AnimationKey.Circle, currentState),
                easing: Easing.SpringOut);

            Animation parentAnimation = new Animation();

            if (currentState == State.Expanded)
            {
                parentAnimation.Add(0.00, 0.50, cutoutAnim);
                parentAnimation.Add(0.50, 0.70, expandAnim);
                parentAnimation.Add(0.70, 0.90, flyupAnim);
                parentAnimation.Add(0.85, 1.00, circleAnim);
            }
            else
            {
                parentAnimation.Add(0.00, 0.15, circleAnim);
                parentAnimation.Add(0.10, 0.30, flyupAnim);
                parentAnimation.Add(0.30, 0.50, expandAnim);
                parentAnimation.Add(0.50, 1.00, cutoutAnim);
            }

            parentAnimation.Commit(this, "ExpandAnimation", 16, 2000);
        }

        private void SkiaOverlay_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            float paddingPx = (float)padding * (float)_density;

            float left = paddingPx;
            float top = (float)(anim[AnimationKey.Cell].Current * _density) + paddingPx;
            float right = info.Width - paddingPx;
            float bottom = top + (float)(cellHeight * _density) - paddingPx;

            // adjust the cutout based on the expand animation
            float expandValue = (float)(anim[AnimationKey.Expand].Current * _density);

            if (expandValue > 0)
            {
                // change the values
                left -= expandValue;
                if (left < 0) left = 0;

                right += expandValue;
                if (right > info.Width) right = info.Width;

                top -= expandValue;
                if (top < 0) top = 0;

                bottom += expandValue;
                if (bottom > info.Height) bottom = info.Height;
            }

            // create a cutout
            var cornerRadius = (paddingPx * 2) - expandValue;

            // store the state of the canvas
            using (new SKAutoCanvasRestore(canvas))
            {

                var cutoutRect = new SKRect(left, top, right, bottom);
                canvas.ClipRoundRect(new SKRoundRect(cutoutRect, cornerRadius, cornerRadius),
                    SKClipOperation.Difference, true);

                // draw the background
                SKRect backgroundRect = new SKRect(0, 0, info.Width, info.Height);
                canvas.DrawBitmap(mapbackground, backgroundRect, BitmapStretch.AspectFill);
            }

            // draw the flyup - but only when it should be
            float flyupPos = (float)(anim[AnimationKey.FlyUp].Current * _density);
            float circleRadius = (float)(anim[AnimationKey.Circle].Current * _density);


            if (flyupPos < info.Height)
            {
                SKPoint circleCenter = new SKPoint(info.Width / 2, flyupPos);

                // draw the center circle
                canvas.DrawCircle(circleCenter, circleRadius - (float)(10 * _density), centerCirclePaint);

                // create a circle clip
                SKPath circlePath = new SKPath();
                circlePath.AddCircle(info.Width / 2, flyupPos, circleRadius);
                canvas.ClipPath(circlePath, SKClipOperation.Difference, true);

                SKRect flyupRect = new SKRect(0, flyupPos, info.Width, info.Height);
                canvas.DrawRect(flyupRect, flyupPaint);
            }

        }

    }


}
