# CSaN-Lab2

Сервер  <-->  Клиент

Скобки [] при реализации не нужны

Нас сервере ханятся файлы
(Хранилище файлов - любая папка (на выбор) )

Формат файла сsv 
----
[super](может не быть) 
(un)cheked 
data1;data2;data3;
----

(Формат файла в данной реализации не учитывается)

Клиент отправляет запрос (текстовая команда)
Сервер отвечает (если файл есть то отправляет и его)

Команды клиента (вводим в консоль)
 Resweight 2019-03-05
 Resweight End(самый последний файл)
 [Super]Resweight 2019-03-05
 [Super]Resweight End(самый последний файл)

Нет файла с такой датой 
 file 2019-09-05 no
 [Super]file 2019-09-05 no
Файл есть
 file 2019-03-05 yes
 [Super]file 2019-03-05 yes 
Ждем получения файла

Если есть несколько файлов с одной датой то отсылаем последний

Выполнит желательно ввиде dll 
т.е входные/выходные данные только строки

Название файла "Resweight 2019-03-05" , "[Super]Resweight 2019-03-05"
Сохранять с таким же именем
