using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Common.Window
{
	public static class WindowConst
	{
		// Целевое разрешение экрана.
		public static readonly Vector2 TargetDestination = new Vector2(1024f, 768f);
	}

	public interface IWindowManager
	{
		/// <summary>
		/// Показать окно.
		/// </summary>
		/// <param name="callback">Коллбек, в который будет возвращен медиатор вновь созданного окна.</param>
		/// <param name="type">Тип создаваемого окна.</param>
		/// <param name="args">Дополнительные аргументы, которые будут переданы окну в момент создания.</param>
		/// <param name="isModal">Признак того, что окно модальное.</param>
		/// <param name="isUnique">Признак того, что окно отображается эксклюзивно.</param>
		/// <param name="container">Контейнер, из которого инжектится окно при создании.</param>
		/// <returns>Возвращает <code>true</code>, если окно может быть успешно создано.</returns>
		bool ShowWindow(Action<IWindow> callback, string type, object[] args = null,
			bool isModal = true, bool isUnique = false, DiContainer container = null);

		/// <summary>
		/// Принудительно закрывает окно, ранее открытое с помощью ShowWindow.
		/// </summary>
		/// <param name="window">Медиатор окна, полученный из ShowWindow.</param>
		void CloseWindow(IWindow window);

		/// <summary>
		/// Принудительно закрыть все окна указанных типов.
		/// </summary>
		/// <param name="args">Список типов закрываемых окон.</param>
		void CloseAll(params Type[] args);

		/// <summary>
		/// Возвращает количество открытых на текущий момент окон.
		/// </summary>
		/// <returns>Количество открытых окон.</returns>
		int GetOpenedWindowsCount();

		/// <summary>
		/// Возвращает количество открытых на текущий момент окон указанного типа.
		/// </summary>
		/// <typeparam name="T">Тип контроллера окна.</typeparam>
		/// <returns>Количество открытых окон указанного типа.</returns>
		int GetOpenedWindowsCount<T>() where T : IWindow;

		/// <summary>
		/// Возвращает реактивное свойство, отражающее количество открытых окон.
		/// </summary>
		/// <returns>Реактивное свойство, отражающее количество открытых окон.</returns>
		IReadOnlyReactiveProperty<int> GetOpenedWindowsCountObservable();

		/// <summary>
		/// Возвращает реактивное сврйоство, отражающее количество открытых окон указанного типа.
		/// </summary>
		/// <typeparam name="T">Тип контроллера окна.</typeparam>
		/// <returns>Реактивное свойтство, отражающее количество открытых окон указанного типа.</returns>
		IReadOnlyReactiveProperty<int> GetOpenedWindowsCountObservable<T>() where T : IWindow;
	}
}