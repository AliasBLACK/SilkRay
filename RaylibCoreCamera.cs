using System;
using System.Numerics;

namespace SilkRay
{
	/// <summary>
	/// Camera2D type, defines a 2d camera
	/// </summary>
	public struct Camera2D(Vector2 offset = default, Vector2 target = default, float rotation = 0.0f, float zoom = 1.0f)
	{
		/// <summary>Camera offset (displacement from target)</summary>
		public Vector2 Offset = offset;
		
		/// <summary>Camera target (rotation and zoom origin)</summary>
		public Vector2 Target = target;
		
		/// <summary>Camera rotation in degrees</summary>
		public float Rotation = rotation;
		
		/// <summary>Camera zoom (scaling), should be 1.0f by default</summary>
		public float Zoom = zoom;

		/// <summary>
		/// Get camera transform matrix (view matrix)
		/// </summary>
		public readonly Matrix4x4 GetMatrix()
		{
			// Create transformation matrix for 2D camera
			// Order: Translate to target -> Rotate -> Scale -> Translate by offset
			
			Matrix4x4 matTranslation = Matrix4x4.CreateTranslation(-Target.X, -Target.Y, 0.0f);
			Matrix4x4 matRotation = Matrix4x4.CreateRotationZ(Rotation * (float)(Math.PI / 180.0f));
			Matrix4x4 matScale = Matrix4x4.CreateScale(Zoom, Zoom, 1.0f);
			Matrix4x4 matOffset = Matrix4x4.CreateTranslation(Offset.X, Offset.Y, 0.0f);

			return matTranslation * matRotation * matScale * matOffset;
		}

		/// <summary>
		/// Get screen to world transformation matrix
		/// </summary>
		public readonly Matrix4x4 GetScreenToWorld()
		{
			Matrix4x4.Invert(GetMatrix(), out Matrix4x4 inverted);
			return inverted;
		}

		/// <summary>
		/// Convert screen position to world position
		/// </summary>
		public readonly Vector2 GetScreenToWorld2D(Vector2 position)
		{
			Matrix4x4 invMatCamera = GetScreenToWorld();
			Vector3 transform = Vector3.Transform(new Vector3(position.X, position.Y, 0), invMatCamera);
			return new Vector2(transform.X, transform.Y);
		}

		/// <summary>
		/// Convert world position to screen position
		/// </summary>
		public readonly Vector2 GetWorldToScreen2D(Vector2 position)
		{
			Matrix4x4 matCamera = GetMatrix();
			Vector3 transform = Vector3.Transform(new Vector3(position.X, position.Y, 0), matCamera);
			return new Vector2(transform.X, transform.Y);
		}
	}

	/// <summary>
	/// Camera management functions
	/// </summary>
	public static class CameraHelper
	{
		/// <summary>
		/// Set camera pan key to combine with mouse movement (free camera)
		/// </summary>
		public static void SetCameraPanControl(int keyPan)
		{
			// Implementation would depend on input system integration
			// For now, this is a placeholder for API compatibility
		}

		/// <summary>
		/// Set camera alt key to combine with mouse movement (free camera)
		/// </summary>
		public static void SetCameraAltControl(int keyAlt)
		{
			// Implementation would depend on input system integration
			// For now, this is a placeholder for API compatibility
		}

		/// <summary>
		/// Set camera smooth zoom key to combine with mouse (free camera)
		/// </summary>
		public static void SetCameraSmoothZoomControl(int keySmoothZoom)
		{
			// Implementation would depend on input system integration
			// For now, this is a placeholder for API compatibility
		}

		/// <summary>
		/// Set camera move controls (1st person and 3rd person cameras)
		/// </summary>
		public static void SetCameraMoveControls(int keyFront, int keyBack, int keyRight, int keyLeft, int keyUp, int keyDown)
		{
			// Implementation would depend on input system integration
			// For now, this is a placeholder for API compatibility
		}
	}
}
