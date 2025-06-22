# Lojistik Entegrasyon ve Takip Sistemi

## Proje Tanıtımı

Bu proje, bir lojistik firması için siparişlerin harici bir sistemden entegrasyonunu, merkezi bir web arayüzü üzerinden takibini ve teslimat durumlarının harici bir API'ye bildirilmesini sağlayan modüler bir otomasyon sistemidir. Proje, Temiz Mimari (Clean Architecture) prensiplerini benimseyerek, iş mantığını altyapı ve sunum katmanlarından ayırarak sürdürülebilir, test edilebilir ve esnek bir yapı sunar.

### Ana Fonksiyonellikler ve Uygulama Katmanları:

1.  **Sipariş Entegrasyonu (`LogisticsIntegration.OrderFetcher.Console`):**
    * **Amaç:** Harici bir "B Şirketi"ne ait Mock SOAP servisi üzerinden yeni sipariş verilerini düzenli olarak çekmek ve yerel veritabanına kaydetmek. Bu sayede, dış kaynaklardan gelen siparişler otomatik olarak sisteme dahil edilir.
    * **Açıklama:** Belirli aralıklarla çalıştırılmak üzere tasarlanmış bir konsol uygulamasıdır.

2.  **Sipariş Takibi ve Yönetimi (`LogisticsIntegration.Web`):**
    * **Amaç:** Sisteme entegre edilen tüm siparişleri listeleyebilmek, detaylarını görüntüleyebilmek ve sipariş durumlarını (Bekliyor, İşleniyor, Teslim Edildi) kullanıcı dostu bir web arayüzü üzerinden güncelleyebilmek.
    * **Açıklama:** Kullanıcıların sipariş verileriyle etkileşime girmesi için geliştirilmiş bir ASP.NET Core MVC web uygulamasıdır.

3.  **Teslimat Bildirimi (`LogisticsIntegration.DeliveryNotifier.Worker`):**
    * **Amaç:** Durumu "Teslim Edildi" olarak güncellenen siparişlerin teslimat bilgilerini, harici "B Şirketi"nin Mock Teslimat API'sine otomatik olarak bildirmek. Bu, teslimat süreçlerinin ilgili dış sistemlerle senkronize kalmasını sağlar.
    * **Açıklama:** Arka planda sürekli çalışan ve bildirim bekleyen teslimatları tespit ederek otomatik olarak API çağrısı yapan bir .NET Worker Servisi'dir.

4.  **Temiz Mimari ve Modüler Tasarım:**
    * **Amaç:** Kod tabanını mantıksal olarak ayrılmış katmanlara bölerek, her bir katmanın belirli bir sorumluluğa sahip olmasını sağlamak. Bu yaklaşım, iş kurallarının altyapı detaylarından bağımsız kalmasını, kodun daha kolay bakımını, test edilmesini ve gelecekteki geliştirmelere açık olmasını sağlar.

## Kullanılan Teknolojiler

* **.NET 6:** Projenin temel geliştirme framework'ü.
* **ASP.NET Core MVC:** Web kullanıcı arayüzü ve sunum katmanı için.
* **.NET Worker Service:** Uzun süreli, arka plan görevlerini yönetmek için.
* **Entity Framework Core (SQLite):** Veritabanı işlemleri, nesne-ilişkisel eşleme (ORM) ve yerel, taşınabilir veritabanı çözümü için.
* **Dependency Injection:** Uygulama genelinde bağımlılıkların yönetimi ve modülerlik için .NET'in yerleşik DI mekanizması.

## Proje Yapısı

Proje, aşağıdaki katmanlara ayrılmış birden fazla projeden oluşmaktadır:

* `LogisticsIntegration.sln`: Ana çözüm dosyası.
* **Shared Libraries (Paylaşılan Kütüphaneler):** 
    * `LogisticsIntegration.Domain`: İş varlıklarını (Entities) ve alan modellerini tanımlar (örn. `Order`, `Delivery`). Temel arayüzler (örn. `IRepository`, `IUnitOfWork`) ve veri transfer nesnelerini (DTOs) içerir.
    * `LogisticsIntegration.Application.Services`: Uygulama katmanı servislerini (örn. `OrderService`, `OrderImportService`, `DeliveryNotificationService`) ve iş akışlarını barındırır.
    * `LogisticsIntegration.Infrastructure`: Veritabanı implementasyonları (Entity Framework Core `DbContext`, Repository implementasyonları) ve genel altyapı bileşenlerini içerir.
    * `LogisticsIntegration.ApiClients`: Harici B Şirketi API'leri ile iletişim kurmak için arayüzleri ve onların Mock implementasyonlarını barındırır.
* **Executable Projects (Çalıştırılabilir Projeler):**
    * `LogisticsIntegration.Web`: Web kullanıcı arayüzünü barındıran ASP.NET Core MVC uygulaması.
    * `LogisticsIntegration.DeliveryNotifier.Worker`: Arka plan teslimat bildirimi görevini yürüten .NET Worker Servisi.
    * `LogisticsIntegration.OrderFetcher.Console`: Harici API'den sipariş verilerini çeken konsol uygulaması.

## Kurulum ve Çalıştırma

Projeyi yerel ortamınızda ayağa kaldırmak için aşağıdaki adımları izleyin:

### Önkoşullar

* .NET 6 SDK
* Tercih edilen bir IDE (Visual Studio 2022 veya Visual Studio Code önerilir)

### Adımlar

1.  **Depoyu Klonlayın:**
    ```bash
    git clone [https://github.com/bilisimlegirisim/LogisticsIntegration.git](https://github.com/bilisimlegirisim/LogisticsIntegration.git)
    cd LogisticsIntegration
    ```

2.  **Veritabanı Dosyası:**
    * Proje, SQLite veritabanı kullanmaktadır. `LogisticsDb.db` veritabanı dosyasının **çözüm kök dizininizin altında bulunan `Data` klasöründe** yer alması beklenir.
    * **Eğer `Db` klasörü ve `LogisticsDb.db` dosyası yoksa:**
        * Çözüm kök dizininde (`LogisticsIntegration.sln` dosyasının olduğu yerde) `Data` adında yeni bir klasör oluşturun.
        * Bu klasörün içine `LogisticsDb.db` adında boş bir dosya oluşturun. Entity Framework Core, uygulama ilk çalıştığında bu dosyayı kullanarak veritabanı şemasını otomatik olarak oluşturacaktır.
    * **ÖNEMLİ: Connection String Ayarlaması**
        * `LogisticsIntegration.Web`, `LogisticsIntegration.DeliveryNotifier.Worker` ve `LogisticsIntegration.OrderFetcher.Console` projelerinin her birinin `appsettings.json` dosyasında `DefaultConnection` için tanımlı olan `Data Source` yolunu, kendi yerel makinenizdeki `LogisticsDb.db` dosyasının **mutlak yolunu** belirtecek şekilde güncelleyiniz.
        * **Örnek:** `"Data Source=C:\\YourLocalPath\\LogisticsIntegration\\Db\\LogisticsDb.db"`
        * Bu adım projenin düzgün çalışması için zorunludur.

3.  **NuGet Paketlerini Geri Yükleyin:**
    * Visual Studio kullanıyorsanız, çözümü açtığınızda gerekli NuGet paketleri otomatik olarak geri yüklenecektir.
    * Komut satırı üzerinden geri yüklemek için, çözüm kök dizininde aşağıdaki komutu çalıştırın:
        ```bash
        dotnet restore
        ```

4.  **Veritabanı Migrations'larını Uygulayın:**
    * Uygulamalar (özellikle `LogisticsIntegration.Web` ve `LogisticsIntegration.OrderFetcher.Console` projelerinin `Main` metodlarında), başlangıçta Entity Framework Core migration'larını otomatik olarak uygulayacak ve veritabanı şemasını oluşturacaktır.

5.  **Uygulamaları Çalıştırın:**
    Bu proje, işlevselliklerinin tamamını görmek için üç ayrı uygulamanın paralel çalışmasını gerektirir:

    * **1. `LogisticsIntegration.Web` (Web Uygulaması):**
        * Visual Studio'da bu projeyi başlangıç projesi olarak ayarlayıp çalıştırın (`F5` tuşu).
        * Veya komut satırından:
            ```bash
            cd LogisticsIntegration.Web
            dotnet run
            ```
        * Uygulama genellikle `https://localhost:70XX` adresinde çalışacaktır (port numarası değişebilir).

    * **2. `LogisticsIntegration.OrderFetcher.Console` (Sipariş Çekici Konsol Uygulaması):**
        * Visual Studio'da projeye sağ tıklayın ve "Debug" -> "Start new instance" seçeneğini kullanın.
        * Veya komut satırından:
            ```bash
            cd LogisticsIntegration.OrderFetcher.Console
            dotnet run
            ```
        * Bu uygulama, `MockCustomerOrderSoapClient` aracılığıyla örnek siparişleri veritabanına ekleyecektir. Konsol çıktısını takip ederek siparişlerin eklenme durumunu görebilirsiniz.

    * **3. `LogisticsIntegration.DeliveryNotifier.Worker` (Teslimat Bildirim Servisi):**
        * Visual Studio'da projeye sağ tıklayın ve "Debug" -> "Start new instance" seçeneğini kullanın.
        * Veya komut satırından:
            ```bash
            cd LogisticsIntegration.DeliveryNotifier.Worker
            dotnet run
            ```
        * Bu servis arka planda sürekli çalışacak ve "Teslim Edildi" statüsündeki, henüz bildirimi yapılmamış siparişleri (`MockCustomerDeliveryApiClient` aracılığıyla) bildirecektir.

## Kullanım Kılavuzu

1.  **Sipariş Listeleme:** `LogisticsIntegration.Web` uygulamasını başlattıktan sonra ana sayfada mevcut siparişlerin bir listesini göreceksiniz.
2.  **Sipariş Ekleme:** `LogisticsIntegration.OrderFetcher.Console` uygulamasını çalıştırdığınızda, mock siparişler veritabanına eklenir ve web arayüzünde "Bekliyor" (Pending) statüsünde görünürler.
3.  **Sipariş Durumu Güncelleme:**
    * Sipariş listesinden bir siparişin "Detaylar" sayfasına gidin.
    * Açılır menüden siparişin yeni durumunu (`İşleniyor` veya `Teslim Edildi`) seçin.
    * Eğer durum `Teslim Edildi` olarak seçilirse, **Plaka** ve **Teslim Eden Kişi** alanları zorunlu hale gelecektir. Bu bilgileri giriniz.
    * "Durumu Güncelle" butonuna tıklayarak değişikliği kaydedin.
4.  **Teslimat Bildirimlerini İzleme:**
    * Bir siparişin durumunu "Teslim Edildi" olarak güncellediğinizde, `LogisticsIntegration.DeliveryNotifier.Worker` servisi bir sonraki çalışma döngüsünde bu teslimatın "B Şirketi"ne bildirimini yapacaktır.
    * Worker servisinin konsol çıktısını izleyerek bildirimlerin başarıyla gönderilip gönderilmediğini kontrol edebilirsiniz.

## İletişim

* Halil (GitHub: https://github.com/bilisimlegirisim)
* E-posta: halilibrahimkamat@gmail.com
