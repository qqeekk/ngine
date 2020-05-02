# Документация по Ngine CLI
## Команда `tune`

Важной особенностью cистемы `Ngine` является возможность автоматически подбирать некоторые гиперпараметры нейронной сети благодаря многократному обучению нейронной сети при использовании алгоритма `RandomSearch` в модификации `Hyperband`. 

___
### Выполняется после:
- `.\Ngine.CommandLine.exe compile`

### Используется вместе с:
- `.\Ngine.CommandLine.exe train`
- `.\Ngine.CommandLine.exe list ambiguities`


## Справка
___
> `.\Ngine.CommandLine.exe tune -?`

```bash
Usage: Ngine.CommandLine tune [options] <ModelPath> <-a|--ambiguities> <-m|--mappings> <-e|--epochs> <-b|--batch> <-vs|--validation-split>

Arguments:
  ModelPath
  -a|--ambiguities
  -m|--mappings
  -e|--epochs
  -b|--batch
  -vs|--validation-split

Options:
  -?|-h|--help            Show help information
```

- Позиционный аргумент `model-path` определяет путь к `h5` файлу модели, полученного в результате выполнения [команды](compile-cli-command.cmd) `.\Ngine.CommandLine.exe compile`
- Позиционный аргумент `ambiguities` задает путь к файлу `ambiguities.yaml`, полученный в результате выполнения [команды](compile-cli-command.cmd)`.\Ngine.CommandLine.exe compile`
- Позиционный аргумент `mappings` задает путь к [файлу проекций](train-cli-command.md)
- Позиционный целочисленный аргумент `epochs` задает количество эпох обучения
- Позиционный целочисленный аргумент `batch-size` задает размер группы (серии) обучения (не должен превышать число эпох) 
- Позиционный вещественный аргумент `validation-split` принимает значения от 0 до 1. Задает коэффициент по которому совершается разделение выборки на тренировочную и тестовую
___

Команда осуществляет поиск наилучших гиперпараметров искусственной нейросети в пространстве [неопрелеленностей](ngine-schema.md), заложенных в документе схемы.

На данный момент оптимизация совершается по метрике `accuracy`. В далнейшем, это поведение может быть дополнено.