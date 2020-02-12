/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;

public interface IReproducible
{
	GameObject FindPotentialMate();
	void RequestMating();
	bool IsAttracted();
	void Mate();
}
