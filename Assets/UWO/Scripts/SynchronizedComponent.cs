﻿using UnityEngine;
using System.Collections;

namespace UWO
{

[RequireComponent(typeof(SynchronizedObject))]
public abstract class SynchronizedComponent: MonoBehaviour
{
	[HideInInspector]
	public string id = System.Guid.Empty.ToString();

	[HideInInspector]
	public bool isAlsoReceiveOnLocal = false;

	private SynchronizedObject syncObject_;
	public SynchronizedObject syncObject
	{
		get { return syncObject_ ?? (syncObject_ = GetComponent<SynchronizedObject>()); }
	}

	public string syncObjectId
	{
		get { return syncObject.id; }
	}

	public bool isLocal
	{
		get { return syncObject.isLocal; }
	}

	public bool isRemote
	{
		get { return syncObject.isRemote; }
	}

	public string prefabPath
	{
		get { return syncObject.prefabPath; }
	}

	public string componentName
	{
		get { return this.GetType().Name; }
	}

	[HideInInspector]
	public float heartBeatDuration = 1f;
	[HideInInspector]
	public float sendFrameRate = 30f;
	public float sendFrequency
	{
		get { return 1f / sendFrameRate; }
	}
	public float easing
	{
		get { return sendFrameRate / 60; } // TODO: use actual framerate
	}

	private string preValue_ = null;
	private string preType_ = null;

	void Awake()
	{
		id = System.Guid.NewGuid().ToString();
	}

	void Start()
	{
		if (isLocal) {
			Synchronizer.RegisterComponent(id, this);
			StartCoroutine(Sync());
			StartCoroutine(HeartBeat());
			OnSend();
		}
	}

	protected virtual void OnLocalUpdate()  {}
	protected virtual void OnRemoteUpdate() {}

	void Update()
	{
		if (isLocal) {
			OnLocalUpdate();
		} else {
			OnRemoteUpdate();
		}
	}

	IEnumerator Sync()
	{
		for (;;) {
			if (isLocal) {
				OnSend();
			}
			yield return new WaitForSeconds(sendFrequency);
		}
	}

	IEnumerator HeartBeat()
	{
		yield return new WaitForEndOfFrame();
		Synchronizer.Send(this, preValue_, preType_);
		for (;;) {
			if (isLocal && !string.IsNullOrEmpty(preValue_) && !string.IsNullOrEmpty(preType_)) {
				Synchronizer.Send(this, preValue_, preType_);
			}
			yield return new WaitForSeconds(heartBeatDuration);
		}
	}

	void OnDestroy()
	{
		Synchronizer.UnregisterComponent(id);
	}

	protected virtual void OnSend() {}

	protected void Send(string value, string type)
	{
		if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(type) && preValue_ != value) {
			preValue_ = value;
			preType_ = type;
			Synchronizer.Send(this, value, type);
		}
	}

	protected void Send(string value)
	{
		Send(value, "string");
	}

	protected void Send(bool value)
	{
		Send(value.AsString(), "bool");
	}

	protected void Send(int value)
	{
		Send(value.AsString(), "int");
	}

	protected void Send(uint value)
	{
		Send(value.AsString(), "uint");
	}

	protected void Send(long value)
	{
		Send(value.AsString(), "long");
	}

	protected void Send(ulong value)
	{
		Send(value.AsString(), "ulong");
	}

	protected void Send(float value)
	{
		Send(value.AsString(), "float");
	}

	protected void Send(Vector2 value)
	{
		Send(value.AsString(), "vector2");
	}

	protected void Send(Vector3 value)
	{
		Send(value.AsString(), "vector3");
	}

	protected void Send(Quaternion value)
	{
		Send(value.AsString(), "quaternion");
	}

	protected virtual void OnReceive(string value)
	{
		Debug.Log("receive string: " + value);
	}

	protected virtual void OnReceive(int value)
	{
		Debug.Log("receive int: " + value.AsString());
	}

	protected virtual void OnReceive(uint value)
	{
		Debug.Log("receive uint: " + value.AsString());
	}

	protected virtual void OnReceive(long value)
	{
		Debug.Log("receive long: " + value.AsString());
	}

	protected virtual void OnReceive(ulong value)
	{
		Debug.Log("receive ulong: " + value.AsString());
	}

	protected virtual void OnReceive(bool value)
	{
		Debug.Log("receive bool: " + value.AsString());
	}

	protected virtual void OnReceive(float value)
	{
		Debug.Log("receive float: " + value.AsString());
	}

	protected virtual void OnReceive(Vector2 value)
	{
		Debug.Log("receive vector2: " + value.AsString());
	}

	protected virtual void OnReceive(Vector3 value)
	{
		Debug.Log("receive vector3: " + value.AsString());
	}

	protected virtual void OnReceive(Quaternion value)
	{
		Debug.Log("receive quaternion: " + value.AsString());
	}

	public void Receive(string value, string type, bool isForceUpdate = false)
	{
		if (isLocal && !isForceUpdate && !isAlsoReceiveOnLocal) return;
		syncObject.NotifyAlive();

		switch (type) {
			case "string":
				OnReceive(value);
				break;
			case "int":
				OnReceive(value.AsInt());
				break;
			case "uint":
				OnReceive(value.AsUint());
				break;
			case "long":
				OnReceive(value.AsLong());
				break;
			case "ulong":
				OnReceive(value.AsUlong());
				break;
			case "bool":
				OnReceive(value.AsBool());
				break;
			case "float":
				OnReceive(value.AsFloat());
				break;
			case "vector2":
				OnReceive(value.AsVector2());
				break;
			case "vector3":
				OnReceive(value.AsVector3());
				break;
			case "quaternion":
				OnReceive(value.AsQuaternion());
				break;
			default:
				Debug.LogWarning("typeof(" + type + ") is invalid type for the value of: " + value);
				break;
		}
	}
}

}
