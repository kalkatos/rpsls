﻿// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

using UnityEngine;

namespace Kalkatos.Firecard.Unity
{
	/// <summary>
	/// Defines a plane with the transform's up direction as normal.
	/// </summary>
	public class DragPlane : MonoBehaviour
	{
		public static DragPlane Instance;

		private void Awake ()
		{
			Instance = this;
		}

		private void OnDrawGizmosSelected ()
		{
			Color oldColor = Gizmos.color; 
			Gizmos.color = Color.magenta;
			Vector3 scale = transform.localScale;
			Vector3 upLeft = transform.TransformPoint(-10 * scale.x, 0, 10 * scale.z);
			Vector3 upRight = transform.TransformPoint(10 * scale.x, 0, 10 * scale.z);
			Vector3 downRight = transform.TransformPoint(10 * scale.x, 0, -10 * scale.z);
			Vector3 downLeft = transform.TransformPoint(-10 * scale.x, 0, -10 * scale.z);
			Gizmos.DrawLine(upLeft, upRight);
			Gizmos.DrawLine(upRight, downRight);
			Gizmos.DrawLine(downRight, downLeft);
			Gizmos.DrawLine(downLeft, upLeft);
			Gizmos.color = oldColor;
		}
	}
}