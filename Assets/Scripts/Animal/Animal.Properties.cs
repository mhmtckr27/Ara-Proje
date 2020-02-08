/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;
using UnityEngine.UI;

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
			AssignColorToBar(value, animalStatsUI.foodSaturationBar);
		}
	}
	public float WaterSaturation
	{
		get => _waterSaturation;
		set
		{
			_waterSaturation = value;
			animalStatsUI.waterSaturationBar.fillAmount = WaterSaturation / maxWaterSaturation;
			AssignColorToBar(value, animalStatsUI.waterSaturationBar);
		}
	}
	public float ReproductiveUrge
	{
		get => _reproductiveUrge;
		set
		{
			_reproductiveUrge = value;
			animalStatsUI.reproductiveUrgeBar.fillAmount = ReproductiveUrge / maxReproductiveUrge;
		}
	}
	public float Energy
	{
		get => _energy;
		set
		{
			_energy = value;
			animalStatsUI.energyBar.fillAmount = Energy / maxEnergy;
			AssignColorToBar(value, animalStatsUI.energyBar);
		}
	}
	public float RemainingLifeTime
	{
		get => _remainingLifeTime;
		set
		{
			_remainingLifeTime = value;
			animalStatsUI.remainingLifeTimeBar.fillAmount = RemainingLifeTime / maxLifeTime;
			AssignColorToBar(value, animalStatsUI.remainingLifeTimeBar);
		}
	}
	public float MoveSpeed
	{
		get => _moveSpeed;
		set
		{
			_moveSpeed = value;
			animalStatsUI.moveSpeedText.text += (value.ToString("F1") + "/" + maxMoveSpeed);  
			navMeshAgent.speed = value;
		}
	}
	public float AngularSpeed
	{
		get => _angularSpeed;
		set
		{
			_angularSpeed = value;
			animalStatsUI.angularSpeedText.text += (value.ToString("F1") + "/" + maxAngularSpeed);
			navMeshAgent.angularSpeed = value;
		}
	}
	public float Acceleration
	{
		get => _acceleration;
		set
		{
			_acceleration = value;
			animalStatsUI.accelerationText.text += (value.ToString("F1") + "/" + maxAcceleration);
			navMeshAgent.acceleration = value;
		}
	}
	public float ExploreTimer 
	{ 
		get => _exploreTimer;
		set
		{
			_exploreTimer = value;
			//animalStatsUI.exploreTimerText.text += (value.ToString("F1") + "/" + maxExploreTimer);
		}
	}
	public float ExploreRadius 
	{ 
		get => _exploreRadius;
		set
		{
			_exploreRadius = value;
			animalStatsUI.exploreRadiusText.text += (value.ToString("F1") + "/" + maxExploreRadius);
		}
	}
	public float EscapeRadius { get => _escapeRadius; set => _escapeRadius = value; }
	public float EscapeTimer { get => _escapeTimer; set => _escapeTimer = value; }


	#endregion

	private void AssignColorToBar(float value, Image bar)
	{
		if (value > criticalLimit)
		{
			bar.color = Color.green;
		}
		else
		{
			bar.color = Color.red;
		}
	}
}
