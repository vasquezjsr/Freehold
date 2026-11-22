using UnityEngine;

namespace Freehold.Art
{
    /// <summary>
    /// Builds a readable low-poly humanoid under the player root.
    /// Limb motion is driven by <see cref="PlayerLocomotionVisual"/> (procedural).
    /// </summary>
    public static class PlayerVisualBuilder
    {
        public static PlayerLocomotionVisual Build(Transform playerRoot)
        {
            var model = new GameObject("PlayerModel");
            model.transform.SetParent(playerRoot, false);
            // CharacterController pivot is near feet; mesh stands on that origin.
            model.transform.localPosition = Vector3.zero;

            var hip = CreateJoint(model.transform, "Hips", new Vector3(0f, 0.95f, 0f));
            var torso = CreateJoint(hip, "Torso", new Vector3(0f, 0.1f, 0f));
            var head = CreateJoint(torso, "Head", new Vector3(0f, 0.38f, 0f));

            // Torso / clothes
            AttachBox(torso, "Chest", new Vector3(0f, 0.18f, 0f), new Vector3(0.42f, 0.45f, 0.24f), ArtPalette.Shirt);
            AttachBox(torso, "Collar", new Vector3(0f, 0.38f, 0.02f), new Vector3(0.36f, 0.08f, 0.22f), Color.Lerp(ArtPalette.Shirt, Color.white, 0.1f));

            // Head + hair
            AttachBox(head, "Skull", new Vector3(0f, 0.12f, 0f), new Vector3(0.28f, 0.28f, 0.26f), ArtPalette.Skin);
            AttachBox(head, "Hair", new Vector3(0f, 0.24f, -0.02f), new Vector3(0.3f, 0.12f, 0.28f), ArtPalette.Hair);
            AttachBox(head, "Nose", new Vector3(0f, 0.1f, 0.14f), new Vector3(0.05f, 0.05f, 0.06f), Color.Lerp(ArtPalette.Skin, Color.black, 0.08f));

            // Legs
            var leftThigh = CreateJoint(hip, "LeftThigh", new Vector3(-0.12f, -0.05f, 0f));
            var leftShin = CreateJoint(leftThigh, "LeftShin", new Vector3(0f, -0.38f, 0f));
            var leftFoot = CreateJoint(leftShin, "LeftFoot", new Vector3(0f, -0.36f, 0.04f));
            AttachBox(leftThigh, "L_Pants", new Vector3(0f, -0.18f, 0f), new Vector3(0.16f, 0.38f, 0.16f), ArtPalette.Pants);
            AttachBox(leftShin, "L_Shin", new Vector3(0f, -0.16f, 0f), new Vector3(0.13f, 0.34f, 0.13f), ArtPalette.Pants);
            AttachBox(leftFoot, "L_Boot", new Vector3(0f, -0.04f, 0.05f), new Vector3(0.14f, 0.1f, 0.24f), ArtPalette.Boots);

            var rightThigh = CreateJoint(hip, "RightThigh", new Vector3(0.12f, -0.05f, 0f));
            var rightShin = CreateJoint(rightThigh, "RightShin", new Vector3(0f, -0.38f, 0f));
            var rightFoot = CreateJoint(rightShin, "RightFoot", new Vector3(0f, -0.36f, 0.04f));
            AttachBox(rightThigh, "R_Pants", new Vector3(0f, -0.18f, 0f), new Vector3(0.16f, 0.38f, 0.16f), ArtPalette.Pants);
            AttachBox(rightShin, "R_Shin", new Vector3(0f, -0.16f, 0f), new Vector3(0.13f, 0.34f, 0.13f), ArtPalette.Pants);
            AttachBox(rightFoot, "R_Boot", new Vector3(0f, -0.04f, 0.05f), new Vector3(0.14f, 0.1f, 0.24f), ArtPalette.Boots);

            // Arms
            var leftUpper = CreateJoint(torso, "LeftUpperArm", new Vector3(-0.28f, 0.28f, 0f));
            var leftLower = CreateJoint(leftUpper, "LeftLowerArm", new Vector3(0f, -0.28f, 0f));
            AttachBox(leftUpper, "L_Sleeve", new Vector3(0f, -0.14f, 0f), new Vector3(0.12f, 0.28f, 0.12f), ArtPalette.Shirt);
            AttachBox(leftLower, "L_Forearm", new Vector3(0f, -0.14f, 0f), new Vector3(0.1f, 0.28f, 0.1f), ArtPalette.Skin);
            AttachBox(leftLower, "L_Hand", new Vector3(0f, -0.3f, 0f), new Vector3(0.1f, 0.08f, 0.1f), ArtPalette.Skin);

            var rightUpper = CreateJoint(torso, "RightUpperArm", new Vector3(0.28f, 0.28f, 0f));
            var rightLower = CreateJoint(rightUpper, "RightLowerArm", new Vector3(0f, -0.28f, 0f));
            AttachBox(rightUpper, "R_Sleeve", new Vector3(0f, -0.14f, 0f), new Vector3(0.12f, 0.28f, 0.12f), ArtPalette.Shirt);
            AttachBox(rightLower, "R_Forearm", new Vector3(0f, -0.14f, 0f), new Vector3(0.1f, 0.28f, 0.1f), ArtPalette.Skin);
            AttachBox(rightLower, "R_Hand", new Vector3(0f, -0.3f, 0f), new Vector3(0.1f, 0.08f, 0.1f), ArtPalette.Skin);

            var anim = playerRoot.gameObject.AddComponent<PlayerLocomotionVisual>();
            anim.Configure(hip, torso, head, leftThigh, leftShin, rightThigh, rightShin, leftUpper, leftLower, rightUpper, rightLower);
            return anim;
        }

        static Transform CreateJoint(Transform parent, string name, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            return go.transform;
        }

        static void AttachBox(Transform parent, string name, Vector3 localPos, Vector3 size, Color color)
        {
            var mesh = LowPolyMeshes.Box(name, size, color);
            MeshBuilder.CreatePart(name, parent, mesh, LowPolyMeshes.Mat(color), localPos, Quaternion.identity, Vector3.one);
        }
    }
}
