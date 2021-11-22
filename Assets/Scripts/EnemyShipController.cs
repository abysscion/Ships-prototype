using System.Collections;
using UnityEngine;

/// <summary>
/// Realizes enemy ship logic
/// </summary>
public class EnemyShipController : Ship
{
	[Header("Enemy ship specific settings")]
	public Transform L_cannonTf;
	public Transform L_shootingPointTf;
	public Transform R_cannonTf;
	public Transform R_shootingPointTf;

	[Header("AI settings")]
	[Tooltip("Possible adittion/reduction to force that launches cannonball")]
	public float possibleForceOffsetOnShoot = 200f;
	[Tooltip("Angle offset of player's look vector that determines AI understanding player aiming")]
	public float maxPlayerAimAngleOffsetToDetectAim = 10f;
	[Tooltip("Delay between shots in seconds")]
	public float shootingDelay = 2f;
	[Tooltip("Distance at which aiming is possible")]
	public float aimingDistance = 12f;
	[Tooltip("Number of shots that AI should make until it would even think about dodging")]
	public int minShotsUntilDodgeAvailable = 2;
	[Range(0, 10)]
	[Tooltip("Maximum possible offset from look vector to determine possibility of aiming")]
	public int maxAngleOffsetOnAim = 5;

	[Header("Debug purpose")]
	[SerializeField]
	[Tooltip("AI current state")]
	private AIState _state;

	/// <summary>
	/// Calls when ship is being destroyed
	/// </summary>
	[HideInInspector]
	public System.Action<GameObject> OnDestroyCallback;

	private Transform _playerShipTf;
	private float _lastShotTime;
	private bool _isAimingFromRightSide;
	private bool _isGoingToDie;

	private enum AIState
    {
		Idle,
		Chasing,
		Aiming,
		Shooting,
		Dodging
    }

	private GameController GCInst => GameController.Instance;
	private Vector3 HostToPlayerVec => _playerShipTf.position - transform.position;
	private bool IsOnAimRange => HostToPlayerVec.magnitude <= aimingDistance;
	private bool IsTargetAimed => Mathf.Abs(GetAimingAngle()) <= maxAngleOffsetOnAim;
	private bool IsAbleToShoot => IsTargetAimed && IsOnAimRange;

	private void Start()
	{
		_rb = GetComponent<Rigidbody>();
		_playerShipTf = GCInst.playerShip.transform;
		GCInst.playerShip.OnPlayerDeath += OnPlayerDeathAction;
		transform.LookAt(_playerShipTf);

		StartCoroutine(ChasePlayer());
	}

    private void OnCollisionEnter(Collision collision)
    {
		if (!collision.gameObject.CompareTag("PlayerCannonball"))
			return;
		if (_isGoingToDie)
			return;

		Destroy(this.gameObject);

		_isGoingToDie = true;
		GCInst.DestroyedEnemies++;
		GCInst.playerShip.OnPlayerDeath -= OnPlayerDeathAction;
		OnDestroyCallback?.Invoke(this.gameObject);
		Debug.Log($"enemy [{gameObject.name}] died at state [{_state}]");
	}

    private IEnumerator ChasePlayer()
    {
		_state = AIState.Chasing;

		while (!IsOnAimRange)
		{
			base.RotateTowards(HostToPlayerVec);
			base.MoveTowards(_playerShipTf.position);
			yield return null;
		}

		StartCoroutine(AimAtPlayer());
	}

	private IEnumerator AimAtPlayer()
    {
		_state = AIState.Aiming;

		var rotationStep = rotationSpeed * Time.deltaTime;
		var isRotationComplete = false;

		_isAimingFromRightSide = Random.Range(1, 3) % 2 == 0;
		while (!isRotationComplete)
		{
			var aimingAngle = GetAimingAngle();
			var sign = aimingAngle >= 0 ? 1f : -1f;

			transform.Rotate(Vector3.up, rotationStep * sign);
			isRotationComplete = Mathf.Abs(aimingAngle) <= maxAngleOffsetOnAim;
			yield return null;
		}

		StartCoroutine(ShootAtPlayer());
	}

	private IEnumerator ShootAtPlayer()
	{
		_state = AIState.Shooting;

		var shotsUntilDodgeLeft = minShotsUntilDodgeAvailable;

		while (IsAbleToShoot && shotsUntilDodgeLeft > 0)
        {
			if ((Time.timeSinceLevelLoad - _lastShotTime) > shootingDelay)
            {
				FireCannon();
				_lastShotTime = Time.timeSinceLevelLoad;
				shotsUntilDodgeLeft--;
            }
			yield return null;
		}

		if (shotsUntilDodgeLeft > 0)
			StartCoroutine(ChasePlayer());
		else if (IsPlayerAimedToHost())
			StartCoroutine(DodgePlayer());
		else
			StartCoroutine(ShootAtPlayer());
	}

	private IEnumerator DodgePlayer()
    {
		_state = AIState.Dodging;

		while (IsPlayerAimedToHost() && HostToPlayerVec.magnitude < aimingDistance)
        {
			base.MoveTowards(transform.position + transform.forward * 5);
			yield return null;
        }

		StartCoroutine(ChasePlayer());
    }

	private void FireCannon()
	{
		var forceValueOffset = Random.Range(-possibleForceOffsetOnShoot, possibleForceOffsetOnShoot);

		if (_isAimingFromRightSide)
			base.LaunchCannonball(R_shootingPointTf.position, GetShootingVec(), forceValueOffset);
		else
			base.LaunchCannonball(L_shootingPointTf.position, GetShootingVec(), forceValueOffset);
	}

	private float GetAimingAngle()
    {
		var aimingVec = _isAimingFromRightSide ? transform.right : -transform.right;
		var angle = Vector3.SignedAngle(aimingVec, HostToPlayerVec, transform.up);

		Debug.DrawRay(transform.position, aimingVec * 10, Color.red);
		Debug.DrawRay(transform.position, HostToPlayerVec, Color.blue);

		return angle;
	}

	private Vector3 GetShootingVec()
    {
		if (_isAimingFromRightSide)
			return (R_shootingPointTf.position - R_cannonTf.position).normalized;
		else
			return (L_shootingPointTf.position - L_cannonTf.position).normalized;
	}

	private bool IsPlayerAimedToHost()
    {
		var playerAimAngle = Vector3.SignedAngle(-HostToPlayerVec, _playerShipTf.forward, _playerShipTf.up);

		return Mathf.Abs(playerAimAngle) <= maxPlayerAimAngleOffsetToDetectAim;
	}

	private void OnPlayerDeathAction()
    {
		this.enabled = false;
    }
}
