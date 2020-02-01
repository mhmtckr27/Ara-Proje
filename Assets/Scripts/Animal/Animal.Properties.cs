/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;

public partial class Animal : MonoBehaviour
{
	#region Basic Animal Attributes Properties
	public float FoodSaturation
	{
		get => _foodSaturation;
		set
		{
			_foodSaturation = value;
			animalStatsUI.foodSaturationBar.fillAmount = _foodSaturation / maxFoodSaturation;
		}
	}
	public float WaterSaturation
	{
		get => _waterSaturation;
		set
		{
			_waterSaturation = value;
			animalStatsUI.waterSaturationBar.fillAmount = WaterSaturation / maxWaterSaturation;
		}
	}
	public float ReproductiveUrge
	{
		get => _reproductiveUrge;
		set
		{
			_reproductiveUrge = value;
			animalStatsUI.reproductionUrgeBar.fillAmount = ReproductiveUrge / maxReproductiveUrge;
		}
	}
	public float Energy
	{
		get => _energy;
		set
		{
			_energy = value;
			animalStatsUI.energyBar.fillAmount = Energy / maxEnergy;
		}
	}
	public float RemainingLifeTime
	{
		get => _remainingLifeTime;
		set
		{
			_remainingLifeTime = value;
			animalStatsUI.remainingLifeTimeBar.fillAmount = RemainingLifeTime / maxLifeTime;
		}
	}
	public float MoveSpeed
	{
		get => _moveSpeed;
		set
		{
			_moveSpeed = value;
			navMeshAgent.speed = value;
		}
	}
	public float AngularSpeed
	{
		get => _angularSpeed;
		set
		{
			_angularSpeed = value;
			navMeshAgent.angularSpeed = value;
		}
	}
	public float Acceleration
	{
		get => _acceleration;
		set
		{
			_acceleration = value;
			navMeshAgent.acceleration = value;
		}
	}
	#endregion
}
