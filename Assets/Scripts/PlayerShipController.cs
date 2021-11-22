using UnityEngine;

/// <summary>
/// Realizes controls over player ship and it's misc logic.
/// </summary>
public class PlayerShipController : Ship
{
	[Header("Player ship specific settings")]
	public Transform shootingPointTf;
	public Transform cannonTf;
	[Tooltip("Delay between new cannonballs loading in seconds")]
	public float cannonballsIncomeDelay = 5f;

	/// <summary>
	/// Calls when cannonball amount changed
	/// </summary>
	public System.Action<int> OnCannonballAmountChanged;
	/// <summary>
	/// Calls when new cannonball is loaded
	/// </summary>
	public System.Action OnCannonballLoad;
	/// <summary>
	/// Calls when player died
	/// </summary>
	public System.Action OnPlayerDeath;

	private Transform _hostTf;
	private Transform _camTf;
	private Camera _cam;
	private float _lastCannonballIncomeTime;
	private int _cannonballsAmount = 5;
	private bool _isDying;

	private Vector3 MouseToPlanePoint
	{
		get
		{
			var mPos = Input.mousePosition;

			mPos.z = _camTf.position.y;
			return _cam.ScreenToWorldPoint(mPos);
		}
	}

	private bool IsMouseWithinViewport
	{
		get
		{
			var mouseToVpPoint = _cam.ScreenToViewportPoint(Input.mousePosition);

			if (mouseToVpPoint.x < 0.03 || mouseToVpPoint.x > 0.97)
				return false;
			if (mouseToVpPoint.y < 0.03 || mouseToVpPoint.y > 0.97)
				return false;
			return true;
		}
	}

	public int CannonballsAmount
    {
		get
        {
			return _cannonballsAmount;
        }
		private set
        {
			_cannonballsAmount = value;
			if (_cannonballsAmount < 0)
				_cannonballsAmount = int.MaxValue;
			OnCannonballAmountChanged?.Invoke(_cannonballsAmount);
        }
    }

	private void Start()
	{
		_cam = Camera.main;
		_camTf = _cam.transform;
		_hostTf = this.transform;
		_rb = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		HandleInput();
		TryToIncreaseCannonballsAmount();
	}

    private void OnCollisionEnter(Collision collision)
    {
		if (_isDying)
			return;
		if (!collision.gameObject.CompareTag("Enemy") &&
			!collision.gameObject.CompareTag("EnemyCannonball"))
			return;

		OnPlayerDeath?.Invoke();
		_isDying = true;
    }

    private void HandleInput()
	{
		HandleLMB_Pressed();
		HandleRMB_Down();
	}

	private void HandleLMB_Pressed()
	{
		if (!Input.GetMouseButton(0))
			return;
		if (!IsMouseWithinViewport)
			return;

		var destVec = MouseToPlanePoint - _hostTf.position;

		base.RotateTowards(destVec);
		base.MoveTowards(MouseToPlanePoint);
	}

	private void HandleRMB_Down()
    {
		if (!Input.GetMouseButtonDown(1))
			return;

		ResolveShooting();
	}

	private void ResolveShooting()
    {
		if (CannonballsAmount <= 0)
			return;

		var shootingVec = (shootingPointTf.position - cannonTf.position).normalized;

		base.LaunchCannonball(shootingPointTf.position, shootingVec);
		CannonballsAmount--;
	}

	private void TryToIncreaseCannonballsAmount()
    {
		if ((Time.timeSinceLevelLoad - _lastCannonballIncomeTime) < cannonballsIncomeDelay)
			return;

		_lastCannonballIncomeTime = Time.timeSinceLevelLoad;
		CannonballsAmount++;
		OnCannonballLoad?.Invoke();
	}
}
