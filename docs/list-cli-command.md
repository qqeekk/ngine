# Документация по Ngine CLI
## Команда `list`

Выводит список синтаксических правил для каждого типа параметров сети в [языке](ngine-schema.md) `ngine-schema`

## Справка
___
> `.\ngine.exe list -?`

```bash
Usage: ngine list [options] <Name>

Arguments:
  Name
                   Allowed values are: Layers, Activations, HeadActivations, Losses, Optimizers, Ambiguities

Options:
  -r|--with-regex  WithRegex
  -?|-h|--help     Show help information
```

Позиционный аргумент `name` определяет область [документации](ngine-schema.md), запрашиваемую для отображения.  Может принимать одно из значений списка:
- `layers` - просмотр синтаксических правил, описывающих типы и свойства **скрытых и входных (`layers`)** слоев нейронной сети
- `activations` - просмотр функций активации допустимых в **скрытых (`layers`)** слоях нейронной сети
- `headActivations` - просмотр функций активации допустимых в **выходных (`heads`)** слоях нейронной сети 
- `losses` - просмотр функций потерь допустимых в **выходных (`heads`)** слоях нейронной сети
- `optimizers` - просмотр синтаксических правил, описывающих допустимые оптимизаторы нейронной сети
- `ambiguities` - просмотр синтаксических правил, описывающих переменные гиперпаметры слоев нейронной сети

Если указан флаг `-r|--with-regex`, то документация будет отображаться в виде регулярных выражений (см. примеры)

## Примеры
____


Оптимизаторы (шаблоны): 

> `.\ngine.exe list optimizers`
```bash
-> sgd: 'sgd({{ learningRate: ufloat }})(, momentum={{ momentum: ufloat }})?(, decay={{ decay: ufloat }})?'
   -> ufloat: 'floating point number: (0; 1)'

-> rmsProp: 'rmsProp({{ learningRate: ufloat }}), rho={{ rho: ufloat }}(, decay={{ decay: ufloat }})?'
   -> ufloat: 'floating point number: (0; 1)'

-> adam: 'adam({{ learningRate: ufloat }}, {{ beta1: ufloat }}, {{ beta2: ufloat }})(, decay={{ decay: ufloat }})?'
   -> ufloat: 'floating point number: (0; 1)'
```


Оптимизаторы (регулярные выражения):

> `.\ngine.exe list optimizers -r`
```bash
~~ sgd: '^sgd\((?<learningRate>\d+(\.\d+)?)\)(, momentum=(?<momentum>\d+(\.\d+)?))?(, decay=(?<decay>\d+(\.\d+)?))?$'
~~ rmsProp: '^rmsProp\((?<learningRate>\d+(\.\d+)?)\), rho=(?<rho>\d+(\.\d+)?)(, decay=(?<decay>\d+(\.\d+)?))?$'
~~ adam: '^adam\((?<learningRate>\d+(\.\d+)?), (?<beta1>\d+(\.\d+)?), (?<beta2>\d+(\.\d+)?)\)(, decay=(?<decay>\d+(\.\d+)?))?$'
```

____
Переменные (шаблоны):

>  `.\ngine.exe list ambiguities`
```bash
-> uint array|uint range: '[({{ list: uint array }}|{{ range: uint range }})]'
   -> uint array: '{{ first: uint }}(,{{ others: uint }})*'
      -> uint: 'positive integer'

   -> uint range: '{{ from: uint }}:{{ end: uint }}:{{ step: uint }}'
      -> uint: 'positive integer'
```
