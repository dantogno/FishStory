using FlatRedBall;
using FlatRedBall.Glue.StateInterpolation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StateInterpolationPlugin;
using System.IO;

namespace FishStory.Entities
{
    public partial class ShaderRenderer
    {
        #region Fields/Properties

        SpriteBatch _spriteBatch;

        public PositionedObject Viewer { get; set; }

        public Texture2D WorldTexture { get; set; }
        public Texture2D LightSourcesTexture { get; set; }
        public Texture2D BackgroundTexture { get; set; }

        public float DarknessAlpha { get; set; }
        private const float MaximumDarknessAlpha = 0.7f;


        private Color _worldColor = Color.White;
        private Color _dayColor = Color.LightGoldenrodYellow;
        private Color _nightColor = Color.MidnightBlue;

        private Vector3 _dayColorAsVector;
        private Vector3 _nightColorAsVector;

        private Tweener _lastColorTween;

        #endregion

        #region Initialize
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
        private void CustomInitialize()
        {
            _spriteBatch = new SpriteBatch(FlatRedBallServices.GraphicsDevice);
            _dayColorAsVector = _dayColor.ToVector3();
            _nightColorAsVector = _nightColor.ToVector3();
        }

        #endregion

        private void CustomActivity()
        {

            //if (InputManager.Keyboard.KeyPushed(Microsoft.Xna.Framework.Input.Keys.Q))
            //    DisplacementStart--;
            //if (InputManager.Keyboard.KeyPushed(Microsoft.Xna.Framework.Input.Keys.E))
            //    DisplacementStart++;
            //Effect.Parameters["DisplacementStart"].SetValue(DisplacementStart);
        }

        private void CustomDestroy()
        {
            SpriteManager.RemoveDrawableBatch(this);
        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
        public void InitializeRenderVariables()
        {
            //Effect.Parameters["BlurStrength"].SetValue(BlurStrength);
            //Effect.Parameters["DisplacementStart"].SetValue(DisplacementStart);
        }

        public void Draw(Camera camera)
        {
            //DrawWorld(camera);

            DrawDarknessToRenderTarget(camera);
            DrawToScreen(camera);
        }

        private void UpdateWorldColor()
        {
            const float secondsToTransitionColor = 5f;
            if (_lastColorTween != null && _lastColorTween.Running) return;

            if (_worldColor == _dayColor && !SunlightManager.SunIsUp && SunlightManager.MoonIsUp)
            {
                _lastColorTween =
                    new Tweener(1, 0, secondsToTransitionColor, InterpolationType.Linear, Easing.InOut)
                    {
                        PositionChanged = HandleColorPositionChanged
                    };

                _lastColorTween.Ended += () =>
                {
                    _worldColor = _nightColor;
                    _lastColorTween.Stop();
                };

                _lastColorTween.Owner = this;

                TweenerManager.Self.Add(_lastColorTween);
                _lastColorTween.Start();
            }
            else if (_worldColor == _nightColor && SunlightManager.SunIsUp)
            {
                _lastColorTween =
                    new Tweener(0, 1, secondsToTransitionColor, InterpolationType.Linear, Easing.InOut)
                    {
                        PositionChanged = HandleColorPositionChanged
                    };

                _lastColorTween.Ended += () =>
                {
                    _worldColor = _dayColor;
                    _lastColorTween.Stop();
                };

                _lastColorTween.Owner = this;

                TweenerManager.Self.Add(_lastColorTween);
                _lastColorTween.Start();
            }
        }

        private void HandleColorPositionChanged(float newposition)
        {
            _worldColor = new Color(_dayColorAsVector * newposition + _nightColorAsVector * (1 - newposition));
        }

        private void DrawToScreen(Camera camera)
        {
            var destinationRectangle = camera.DestinationRectangle;

            FlatRedBallServices.GraphicsDevice.SetRenderTarget(null);

            UpdateWorldColor();

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            //_spriteBatch.Draw(BackgroundTexture, destinationRectangle, Color.White);
            _spriteBatch.Draw(WorldTexture, destinationRectangle, _worldColor);
            _spriteBatch.Draw(NightDarknessRenderTarget, destinationRectangle, Color.White);
            _spriteBatch.End();

            FlatRedBallServices.GraphicsDevice.SetRenderTarget(null);
        }

        private void DrawDarknessToRenderTarget(Camera camera)
        {
            DarknessAlpha = MaximumDarknessAlpha * (1 - SunlightManager.SunlightEffectiveness);

            var destinationRectangle = camera.DestinationRectangle;

            FlatRedBallServices.GraphicsDevice.SetRenderTarget(NightDarknessRenderTarget);
            FlatRedBallServices.GraphicsDevice.Clear(new Color(0, 0, 0, 0));

            var darknessColor = new Color(0, 0, 0, DarknessAlpha);

            //First draw the objects as blackness
            _spriteBatch.Begin(SpriteSortMode.Immediate);
            _spriteBatch.Draw(WorldTexture, destinationRectangle, darknessColor);
            _spriteBatch.End();

            //using (Stream stream = System.IO.File.Create("worldtexture.png"))
            //{
            //    NightDarknessRenderTarget.SaveAsPng(stream, destinationRectangle.Width, destinationRectangle.Height);
            //}

            var blendState = new BlendState
            {
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,
            };

            //using (Stream stream = System.IO.File.Create("DarknessTexture.png"))
            //{
            //    DarknessTexture.SaveAsPng(stream, destinationRectangle.Width, destinationRectangle.Height);
            //}

            //Then subtract darkness where light sources are at
            _spriteBatch.Begin(SpriteSortMode.Immediate, blendState);
            _spriteBatch.Draw(LightSourcesTexture, destinationRectangle, new Color(1, 1, 1, DarknessAlpha * 2));
            _spriteBatch.End();

            //using (Stream stream = System.IO.File.Create("result.png"))
            //{
            //    NightDarknessRenderTarget.SaveAsPng(stream, destinationRectangle.Width, destinationRectangle.Height);
            //}
        }
    }
}
