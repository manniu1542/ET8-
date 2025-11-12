using Lockstep.Math;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSOperaComponent))]
    [FriendOf(typeof(LSClientUpdater))]
    public static partial class LSOperaComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.LSOperaComponent self)
        {
        }

        [EntitySystem]
        private static void Update(this LSOperaComponent self)
        {
            LVector2 v = new();
            if (Input.GetKey(KeyCode.W))
            {
                v.y += 1;
            }

            if (Input.GetKey(KeyCode.A))
            {
                v.x -= 1;
            }

            if (Input.GetKey(KeyCode.S))
            {
                v.y -= 1;
            }

            if (Input.GetKey(KeyCode.D))
            {
                v.x += 1;
            }

            LSInputButton button = LSInputButton.None;
            if (Input.GetKey(KeyCode.Space))
            {
                button = button.AddButton(LSInputButton.Jump);
            }

            if (Input.GetKey(KeyCode.J))
            {
                button = button.AddButton(LSInputButton.AttackJ);
            }

            if (Input.GetKey(KeyCode.K))
            {
                button = button.AddButton(LSInputButton.AttackK);
            }

            if (Input.GetKey(KeyCode.L))
            {
                button = button.AddButton(LSInputButton.AttackL);
            }

            LSClientUpdater lsClientUpdater = self.GetParent<Room>().GetComponent<LSClientUpdater>();
            lsClientUpdater.Input.V = v.normalized;
            lsClientUpdater.Input.Button = (int)button;
        }
    }
}