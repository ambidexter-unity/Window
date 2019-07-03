using System;
using UnityEngine;

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
		/// <returns>Возвращает <code>true</code>, если окно может быть успешно создано.</returns>
		bool ShowWindow(Action<IWindow> callback, string type, object[] args = null,
			bool isModal = true, bool isUnique = false);

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
	}
}