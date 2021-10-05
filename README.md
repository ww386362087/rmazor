####  Ресурсы проекта
- [Репозиторий клиента на Github](https://github.com/semi92art/Clickers)
- [Репозиторий RestAPI на Github](https://github.com/semi92art/ClickersAPI)
- Репозиторий RestAPI на DockerHub: `drago1/clickersapi`
- [Логи RestAPI на сервере](http://77.37.152.15:9000/#/home), Логин: `vip_admin`, Пароль: `anthony1980`
- [Zeplin](https://zpl.io/aXB9Rzx)
- Музыка и звуковые эффекты:
	1. [freemusicarchive](https://freemusicarchive.org/home)
	2. [getbeat](https://getbeat.ru/)
	3. [dl-sounds](https://www.dl-sounds.com/royalty-free/category/game-film/video-game/)
	4. [videvo](https://www.videvo.net/)
	5. [storyblocks](https://www.storyblocks.com/audio)
	6. [8 bit generator](https://sfxr.me/)

---

#### Коды ошибок
В коде клиента константы кодов ошибок хранятся в классе `ServerErrorCodes`.
|  Код   | Сообщение                                                     | В каком случае может возникнуть                                                      |
| :-- | :----------------------------------------------------------- | :----------------------------------------------------------- |
|  1  | AccountEntity не найден по DeviceId | При поиске аккаунта по Id устройства |
|  2  | Неверные логин или пароль | При попытке входа по логину и паролю |
|  3  | Entity с таким AccountId и GameId не найден | В случае, если в таблице DataFieldValue нет совпадений по полям AccountId и GameId |
|  4  | Запрос составлен неправильно | В случае неправильного составления запроса к WebAPI |
|  5  | Валидация базы данных завершилась неудачей | При перезаписи полей любой из таблиц БД |
|  6  | Account с таким Name уже существует | При регистрации аккаутна по логину и паролю |
| 7    | Account с таким DeviceId уже существует     | При регистрации гостевого аккаунта по Id устройства          |
| 8    | -                                           | -                                                            |
| 9    | -                                           | -                                                            |
| 10   | -                                           | -                                                            |
| 11   | -                                           | -                                                            |
|  12  | - | -                                                            |

### Clickers API
####  Команды Ubuntu
- Системный мониторинг: `glances`
- Убить процесс через PID: `sudo kill -9 [PID]`
- Запустить процесс в бэкграунде, который будет выполнять файл каждые 60 секунд: `setsid forever watch -n 60 [путь к файлу]`
- Отобразить информацию о процессе, запущенном в бэкграунде, выполняющем файл: `ps -auxwww --sort=start_time | grep -i [имя файла]`
##### Удаление пользователя
1. Ищем все процессы, запущенные пользователем через `ps -aux | grep [имя пользователя]`
2. Удостоверяемся, что не запущены процессы, влияющие на работу системы и убиваем их через `sudo pkill -u [имя пользователя]`
3. Еще раз проверяем, остались ли запущенные пользователем процессы с помощью команды из п.1. Если таковые остались, убиваем каждый через `sudo kill -9 [PID процесса]`
4. Удаляем пользователя с помощью команды `sudo deluser [имя пользователя]`
#### Команды на боевом сервере
- Пул образа из DockerHub + запуск: `/home/artem/Documents/refresh_clapi1.sh`

#### Команды DotNet EF Core
- Создание миграции: `dotnet ef migrations add MigrationName`
- Обновление БД по миграции: `dotnet ef database update MigrationName`
- Обновление утилит dotnet: `dotnet tool update --global dotnet-ef`