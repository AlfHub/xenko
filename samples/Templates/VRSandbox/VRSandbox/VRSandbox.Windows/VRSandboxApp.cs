using System;
using SiliconStudio.Xenko.Engine;

namespace VRSandbox
{
    class VRSandboxApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                //VR needs to run at 90 fps, vsync must be disabled, draw must be not synchronized
                //You might want to set physics time step to 90 fps as well if you use character controller with unregular movements, but please avoid that! use Kinematic rigidbodies when possible.
                game.IsFixedTimeStep = true;
                game.IsDrawDesynchronized = true;
                game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
                game.TargetElapsedTime = TimeSpan.FromSeconds(1 / 90.0f);
                game.Run();
            }
        }
    }
}
