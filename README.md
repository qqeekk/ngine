# Конструктор сверточных нейронных сетей Ngine

Проект **`Ngine`** создан для того, чтобы упростить процессы _создания_, _обучения_ и _запуска_ моделей искусственных нейронных сетей любой сложности, благодаря использованию специализированного языка разметки схем `ngine-schema`, разработанного на основе `yaml`.

С использованием **`Ngine`**, Вам не потребуются навыки программирования для создания полностью рабочих моделей нейронных сетей.

## Установка (раздел ожидает завершения)
---


### Системные требования
- Windows 7+, Linux x64, Mac OS


## Описание вариантов использования программы:
---
Ниже представлены ссылки на документацию по вариантам использования консольного приложения `Ngine.CommandLine.exe`:

- `.\Ngine.CommandLine.exe -?`
    
    просмотр списка допустимых команд

- `.\Ngine.CommandLine.exe list -?` [(подробнее)](./docs/list-cli-command.md) 
  
    просмотр встроенной документации по примитивам `ngine-schema`

- `.\Ngine.CommandLine.exe compile -?` [(подробнее)](./docs/compile-cli-command.md) 
  
    синтаксический и семантический разбор модели, статический анализ, сборка и конвертация в формат `.h5`, совместимый с `tensorflow/keras`
  
- `.\Ngine.CommandLine.exe train -?` [(подробнее)](./docs/train-cli-command.md) 
    
    обучение модели на тренировочных данных

- `.\Ngine.CommandLine.exe tune -?` (подробнее)

     полу-автоматическая настройка гиперпараметров сети 

