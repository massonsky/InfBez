import os
import sys

def get_file_size(filepath):
    """
    Определяет точный размер файла и возвращает его в различных форматах
    """
    try:
        # Проверяем существование файла
        if not os.path.exists(filepath):
            print(f"Ошибка: Файл '{filepath}' не найден")
            return None
        
        # Проверяем, что это файл, а не директория
        if os.path.isdir(filepath):
            print(f"Ошибка: '{filepath}' является директорией, а не файлом")
            return None
        
        # Получаем размер в байтах
        size_bytes = os.path.getsize(filepath)
        
        # Предупреждение о пустом файле
        if size_bytes == 0:
            print(f"\n⚠️  ВНИМАНИЕ: Файл имеет нулевой размер (пустой или повреждён)")
        
        # Конвертируем в различные единицы измерения
        size_kb = size_bytes / 1024
        size_mb = size_kb / 1024
        size_gb = size_mb / 1024
        
        return {
            'bytes': size_bytes,
            'kb': size_kb,
            'mb': size_mb,
            'gb': size_gb
        }
    
    except Exception as e:
        print(f"Ошибка при получении размера файла: {e}")
        return None

def format_size(size_dict):
    """
    Форматирует вывод размера файла
    """
    if size_dict is None:
        return
    
    print(f"\n{'='*50}")
    print(f"Точный размер файла:")
    print(f"{'='*50}")
    print(f"Байты:     {size_dict['bytes']:,} bytes")
    print(f"Килобайты: {size_dict['kb']:.2f} KB")
    print(f"Мегабайты: {size_dict['mb']:.2f} MB")
    print(f"Гигабайты: {size_dict['gb']:.6f} GB")
    print(f"{'='*50}\n")

def main():
    # Проверяем аргументы командной строки
    if len(sys.argv) > 1:
        filepath = sys.argv[1]
    else:
        # Запрашиваем путь к файлу у пользователя
        filepath = input("Введите путь к Word файлу (.doc, .docx): ").strip()
        # Удаляем кавычки, если они есть
        filepath = filepath.strip('"').strip("'")
    
    # Проверяем расширение файла
    valid_extensions = ['.doc', '.docx']
    file_ext = os.path.splitext(filepath)[1].lower()
    
    if file_ext not in valid_extensions:
        print(f"Предупреждение: Файл имеет расширение '{file_ext}'")
        print(f"Ожидаются расширения: {', '.join(valid_extensions)}")
        response = input("Продолжить? (y/n): ")
        if response.lower() != 'y':
            print("Операция отменена")
            return
    
    # Получаем и выводим размер файла
    size_info = get_file_size(filepath)
    
    if size_info:
        format_size(size_info)
        
        # Дополнительная информация о файле
        print(f"Имя файла: {os.path.basename(filepath)}")
        print(f"Полный путь: {os.path.abspath(filepath)}")
        
        # Дополнительная диагностика для пустых или подозрительных файлов
        if size_info['bytes'] == 0:
            print("\n" + "="*50)
            print("ДИАГНОСТИКА:")
            print("="*50)
            print("Возможные причины нулевого размера:")
            print("1. Файл был только что создан и пуст")
            print("2. Файл повреждён")
            print("3. Файл не является настоящим Word документом")
            print("4. Проблема с файловой системой")
            print("\nПопробуйте:")
            print("- Открыть файл в Microsoft Word")
            print("- Проверить права доступа к файлу")
            print("- Создать новый документ для теста")
        elif size_info['bytes'] < 1024:
            print(f"\n⚠️  Файл очень маленький ({size_info['bytes']} байт)")
            print("Обычный Word документ имеет минимум ~3-5 KB")

if __name__ == "__main__":
    main()