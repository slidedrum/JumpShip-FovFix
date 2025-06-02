using UnityEngine;
using MelonLoader;

namespace FovFix
{
    public class FovFixMain :MelonMod
    {
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Camera.main != null && Camera.main.fieldOfView == 65)
            {
                Camera.main.fieldOfView = 105;
            }
        }
    }
}