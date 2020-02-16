﻿/*
Code by Hayri Cakir
www.hayricakir.com
*/
using System;
using UnityEngine;
using UnityEngine.UI;

public partial class Animal
{
	#region Basic Animal Attributes Properties
	public float FoodSaturation
	{
		get => _foodSaturation;
		set
		{
			_foodSaturation = value > MAXFoodSaturation ? MAXFoodSaturation : value;
			animalStatsUI.foodSaturationBar.fillAmount = _foodSaturation / MAXFoodSaturation;
			AssignColorToBar(value, animalStatsUI.foodSaturationBar);
		}
	}
	public float WaterSaturation
	{
		get => _waterSaturation;
		set
		{
			_waterSaturation = value > MAXWaterSaturation ? MAXWaterSaturation : value;
			animalStatsUI.waterSaturationBar.fillAmount = WaterSaturation / MAXWaterSaturation;
			AssignColorToBar(value, animalStatsUI.waterSaturationBar);
		}
	}
	public float ReproductiveUrge
	{
		get => _reproductiveUrge;
		set
		{
			_reproductiveUrge = value > MAXReproductiveUrge ? MAXReproductiveUrge : value;
			animalStatsUI.reproductiveUrgeBar.fillAmount = ReproductiveUrge / MAXReproductiveUrge;
		}
	}
	public float EnergySaturation
	{
		get => _energySaturation;
		set
		{
			_energySaturation = value > MAXEnergySaturation ? MAXEnergySaturation : value;
			animalStatsUI.energyBar.fillAmount = EnergySaturation / MAXEnergySaturation;
			AssignColorToBar(value, animalStatsUI.energyBar);
		}
	}
	public float MoveSpeed
	{
		get => _moveSpeed;
		set
		{
			_moveSpeed = value;
			animalStatsUI.moveSpeedText.text += (value.ToString("F1") + "/" + MAXMoveSpeed);  
			navMeshAgent.speed = value;
		}
	}
	public float AngularSpeed
	{
		get => _angularSpeed;
		set
		{
			_angularSpeed = value;
			animalStatsUI.angularSpeedText.text += (value.ToString("F1") + "/" + MAXAngularSpeed);
			navMeshAgent.angularSpeed = value;
		}
	}
	public float Acceleration
	{
		get => _acceleration;
		set
		{
			_acceleration = value;
			animalStatsUI.accelerationText.text += (value.ToString("F1") + "/" + MAXAcceleration);
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
			animalStatsUI.exploreRadiusText.text += (value.ToString("F1") + "/" + MAXExploreRadius);
		}
	}
	public float Charisma 
	{ 
		get => _charisma;
		set
		{
			_charisma = value;
		}
	}
	public float CurrentFoodIntake 
	{ 
		get => currentFoodIntake;
		set
		{
			currentFoodIntake = value;
			FoodSaturation = currentFoodIntake / foodIntakeNeed * MAXFoodSaturation;
		}
	}
	public float CurrentWaterIntake 
	{ 
		get => currentWaterIntake;
		set
		{
			currentWaterIntake = value;
			WaterSaturation = currentWaterIntake / waterIntakeNeed * MAXWaterSaturation;
		}
	}
	public float CurrentEnergy 
	{ 
		get => currentEnergy;
		set
		{
			currentEnergy = value;
			EnergySaturation = currentEnergy / energyIntakeNeed * MAXEnergySaturation;
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
