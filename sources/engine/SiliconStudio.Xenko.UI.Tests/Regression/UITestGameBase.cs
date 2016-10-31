﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Regression
{
    /// <summary>
    /// A base class for rendering tests
    /// </summary>
    public class UITestGameBase : GameTestBase
    {
        protected readonly Logger Logger = GlobalLogger.GetLogger("Test Game");
        
        private Vector2 lastTouchPosition;

        protected readonly CameraRendererModeForward SceneCameraRenderer = new CameraRendererModeForward { Name = "Camera Renderers" };

        protected Scene Scene;
        protected Entity Camera = new Entity("Scene camera") { new CameraComponent() };
        protected Entity UIRoot = new Entity("Root entity of camera UI") { new UIComponent()  };

        private readonly SceneGraphicsCompositorLayers graphicsCompositor;

        protected UIComponent UIComponent => UIRoot.Get<UIComponent>();

        protected CameraComponent CameraComponent
        {
            get { return Camera.Get<CameraComponent>(); }
            set
            {
                var previousFound = false;
                for (var i = 0; i < Camera.Components.Count; i++)
                {
                    var cameraComponent = Camera.Components[i] as CameraComponent;
                    if (cameraComponent == null)
                        continue;

                    previousFound = true;
                    if (cameraComponent != value)
                    {
                        if (value == null)
                        {
                            Camera.Components.RemoveAt(i);
                        }
                        else
                        {
                            Camera.Components[i] = value;
                        }
                    }
                    break;
                }

                if (!previousFound && value != null)
                {
                    Camera.Add(value);
                }

                graphicsCompositor.Cameras[0] = value;
            }
        }

        /// <summary>
        /// Gets the UI system.
        /// </summary>
        /// <value>The UI.</value>
        protected UISystem UI { get; }

        /// <summary>
        /// Create an instance of the game test
        /// </summary>
        public UITestGameBase()
        {
            StopOnFrameCount = -1;

            graphicsCompositor = new SceneGraphicsCompositorLayers
            {
                Cameras = { Camera.Get<CameraComponent>() },
                Master =
                {
                    Renderers =
                    {
                        new ClearRenderFrameRenderer { Color = Color.Green, Name = "Clear frame" },

                        new SceneDelegateRenderer(SpecificDrawBeforeUI) { Name = "Delegate before main UI" },

                        new SceneCameraRenderer { Mode = SceneCameraRenderer },
                    }
                }
            };
            Scene = new Scene { Settings = { GraphicsCompositor = graphicsCompositor } };

            Scene.Entities.Add(UIRoot);
            Scene.Entities.Add(Camera);

            Camera.Transform.Position = new Vector3(0, 0, 1000);

            UIComponent.IsFullScreen = true;
            UIComponent.Resolution = new Vector3(1000, 500, 500);
            UIComponent.ResolutionStretch = ResolutionStretch.FixedWidthFixedHeight;

            UI = (UISystem)Services.GetService(typeof(UISystem));
            if (UI == null)
            {
                UI = new UISystem(Services);
                GameSystems.Add(UI);
            }
        }

        protected virtual void SpecificDrawBeforeUI(RenderDrawContext context, RenderFrame renderFrame)
        {
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Default styles
            // Note: this is temporary and should be replaced with default template of UI elements
            textBlockTextColor = Color.LightGray;

            scrollingTextTextColor = Color.LightGray;

            var buttonPressedTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ButtonPressed);
            var buttonNotPressedTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ButtonNotPressed);
            var buttonOverredTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ButtonOverred);
            buttonPressedImage = (SpriteFromTexture)new Sprite("Test button pressed design", buttonPressedTexture) { Borders = 8 * Vector4.One };
            buttonNotPressedImage = (SpriteFromTexture)new Sprite("Test button not pressed design", buttonNotPressedTexture) { Borders = 8 * Vector4.One };
            buttonMouseOverImage = (SpriteFromTexture)new Sprite("Test button overred design", buttonOverredTexture) { Borders = 8 * Vector4.One };

            var editActiveTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.EditTextActive);
            var editInactiveTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.EditTextInactive);
            var editOverredTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.EditTextOverred);
            editTextTextColor = Color.LightGray;
            editTextSelectionColor = Color.FromAbgr(0x623574FF);
            editTextCaretColor = Color.FromAbgr(0xF0F0F0FF);
            editTextActiveImage = (SpriteFromTexture)new Sprite("Test edit active design", editActiveTexture) { Borders = 12 * Vector4.One };
            editTextInactiveImage = (SpriteFromTexture)new Sprite("Test edit inactive design", editInactiveTexture) { Borders = 12 * Vector4.One };
            editTextMouseOverImage = (SpriteFromTexture)new Sprite("Test edit overred design", editOverredTexture) { Borders = 12 * Vector4.One };

            var toggleButtonChecked = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ToggleButtonChecked);
            var toggleButtonUnchecked = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ToggleButtonUnchecked);
            var toggleButtonIndeterminate = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ToggleButtonIndeterminate);
            toggleButtonCheckedImage = (SpriteFromTexture)new Sprite("Test toggle button checked design", toggleButtonChecked) { Borders = 8 * Vector4.One };
            toggleButtonUncheckedImage = (SpriteFromTexture)new Sprite("Test toggle button unchecked design", toggleButtonUnchecked) { Borders = 8 * Vector4.One };
            toggleButtonIndeterminateImage = (SpriteFromTexture)new Sprite("Test toggle button indeterminate design", toggleButtonIndeterminate) { Borders = 8 * Vector4.One };

            var designsTexture = TextureExtensions.FromFileData(GraphicsDevice, DefaultDesigns.Designs);
            sliderTrackBackgroundImage = (SpriteFromTexture)new Sprite("Default slider track background design", designsTexture) { Borders = 14 * Vector4.One, Region = new RectangleF(207, 3, 32, 32) };
            sliderTrackForegroundImage = (SpriteFromTexture)new Sprite("Default slider track foreground design", designsTexture) { Borders = 0 * Vector4.One, Region = new RectangleF(3, 37, 32, 32) };
            sliderThumbImage = (SpriteFromTexture)new Sprite("Default slider thumb design", designsTexture) { Borders = 4 * Vector4.One, Region = new RectangleF(37, 37, 16, 32) };
            sliderMouseOverThumbImage = (SpriteFromTexture)new Sprite("Default slider thumb overred design", designsTexture) { Borders = 4 * Vector4.One, Region = new RectangleF(71, 37, 16, 32) };
            sliderTickImage = (SpriteFromTexture)new Sprite("Default slider track foreground design", designsTexture) { Region = new RectangleF(245, 3, 3, 6) };
            sliderTickOffset = 13f;
            sliderTrackStartingOffsets = new Vector2(3);

            Window.IsMouseVisible = true;

            SceneSystem.SceneInstance = new SceneInstance(Services, Scene);
        }

        #region Temporary Fix (Style)
        // Button
        private ISpriteProvider buttonPressedImage;
        private ISpriteProvider buttonNotPressedImage;
        private ISpriteProvider buttonMouseOverImage;
        // Edit Text
        private Color editTextTextColor;
        private Color editTextSelectionColor;
        private Color editTextCaretColor;
        private ISpriteProvider editTextActiveImage;
        private ISpriteProvider editTextInactiveImage;
        private ISpriteProvider editTextMouseOverImage;
        // ScrollingText
        private Color scrollingTextTextColor;
        // Slider
        private ISpriteProvider sliderTrackBackgroundImage;
        private ISpriteProvider sliderTrackForegroundImage;
        private ISpriteProvider sliderThumbImage;
        private ISpriteProvider sliderMouseOverThumbImage;
        private ISpriteProvider sliderTickImage;
        private float sliderTickOffset;
        private Vector2 sliderTrackStartingOffsets;
        // TextBlock
        private Color textBlockTextColor;
        // ToggleButton
        private ISpriteProvider toggleButtonCheckedImage;
        private ISpriteProvider toggleButtonUncheckedImage;
        private ISpriteProvider toggleButtonIndeterminateImage;

        protected void ApplyButtonDefaultStyle(Button button)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));

            button.PressedImage = buttonPressedImage;
            button.NotPressedImage = buttonNotPressedImage;
            button.MouseOverImage = buttonMouseOverImage;
        }

        protected void ApplyEditTextDefaultStyle(EditText editText)
        {
            if (editText == null) throw new ArgumentNullException(nameof(editText));

            editText.TextColor = editTextTextColor;
            editText.SelectionColor = editTextSelectionColor;
            editText.CaretColor = editTextCaretColor;
            editText.ActiveImage = editTextActiveImage;
            editText.InactiveImage = editTextInactiveImage;
            editText.MouseOverImage = editTextMouseOverImage;
        }

        protected void ApplyScrollingTextDefaultStyle(ScrollingText scollingText)
        {
            if (scollingText == null) throw new ArgumentNullException(nameof(scollingText));

            ApplyTextBlockDefaultStyle(scollingText);
            scollingText.TextColor = scrollingTextTextColor;
        }

        protected void ApplySliderDefaultStyle(Slider slider)
        {
            if (slider == null) throw new ArgumentNullException(nameof(slider));

            slider.TrackBackgroundImage = sliderTrackBackgroundImage;
            slider.TrackForegroundImage = sliderTrackForegroundImage;
            slider.ThumbImage = sliderThumbImage;
            slider.MouseOverThumbImage = sliderMouseOverThumbImage;
            slider.TickImage = sliderTickImage;
            slider.TickOffset = sliderTickOffset;
            slider.TrackStartingOffsets = sliderTrackStartingOffsets;
        }

        protected void ApplyTextBlockDefaultStyle(TextBlock textBlock)
        {
            if (textBlock == null) throw new ArgumentNullException(nameof(textBlock));

            textBlock.TextColor = textBlockTextColor;
        }

        protected void ApplyToggleButtonBlockDefaultStyle(ToggleButton toggleButton)
        {
            if (toggleButton == null) throw new ArgumentNullException(nameof(toggleButton));

            toggleButton.CheckedImage = toggleButtonCheckedImage;
            toggleButton.UncheckedImage = toggleButtonUncheckedImage;
            toggleButton.IndeterminateImage = toggleButtonIndeterminateImage;
        }
        #endregion // Temporary Fix (Style)

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (gameTime.FrameCount == StopOnFrameCount || Input.IsKeyDown(Keys.Escape))
                Exit();
        }

        protected PointerEvent CreatePointerEvent(PointerState state, Vector2 position)
        {
            if (state == PointerState.Down)
                lastTouchPosition = position;

            var pointerEvent = new PointerEvent(0, position, position - lastTouchPosition, new TimeSpan(), state, PointerType.Touch, true);

            lastTouchPosition = position;

            return pointerEvent;
        }
    }
}
