# ?? Git Versiyon Yönetimi Rehberi - KaynakMakinesi Projesi

## ?? Ýçindekiler
1. [Temel Git Komutlarý](#temel-git-komutlarý)
2. [Commit Arasý Geçiþ](#commit-arasý-geçiþ)
3. [Geri Yükleme Stratejileri](#geri-yükleme-stratejileri)
4. [Branch Stratejisi](#branch-stratejisi)
5. [Acil Durum Senaryolarý](#acil-durum-senaryolarý)
6. [Best Practices](#best-practices)

---

## ?? Temel Git Komutlarý

### Durum Kontrolü
```bash
# Mevcut durum
git status

# Son 10 commit'i görüntüle
git log --oneline -10

# Grafik olarak göster
git log --oneline --graph --all -10

# Detaylý commit bilgisi
git show <commit-hash>
```

### Deðiþiklikleri Görüntüleme
```bash
# Henüz commit edilmemiþ deðiþiklikler
git diff

# Belirli bir dosyadaki deðiþiklikler
git diff KaynakMakinesi.UI/Program.cs

# Ýki commit arasý farklar
git diff 79a5a4d..47051c5
```

---

## ?? Commit Arasý Geçiþ

### 1. Geçici Görüntüleme (Detached HEAD)
Kodunuzu deðiþtirmeden eski bir commit'i incelemek için:

```bash
# Belirli bir commit'e git (sadece görüntüleme)
git checkout 79a5a4d

# NOT: "detached HEAD" uyarýsý alýrsýnýz - bu normal!
# Kodlarý inceleyebilir, derleyebilirsiniz ama commit yapamazsýnýz

# Geri dönmek için:
git checkout master
```

**Kullaným Senaryosu:**
- "Önceki versiyonda bu özellik nasýl çalýþýyordu?" sorusunu cevaplamak
- Eski bir sürümü test etmek
- Belirli bir commit'teki kodu okumak

---

### 2. Güvenli Branch Oluþturarak Geçiþ
Eski bir commit üzerinde çalýþmak istiyorsanýz:

```bash
# Eski commit'ten yeni bir branch oluþtur
git checkout -b test-old-version 79a5a4d

# Bu branch üzerinde çalýþabilirsiniz
# Deðiþiklikler bu branch'te kalýr

# Master'a geri dönmek için:
git checkout master
```

**Kullaným Senaryosu:**
- Eski bir versiyon üzerinde deneysel çalýþma yapmak
- Bug fix için eski bir sürümden baþlamak
- Farklý yaklaþýmlarý test etmek

---

### 3. Belirli Dosyayý Eski Haline Getirme
Sadece bir dosyayý eski haline getirmek için:

```bash
# Belirli bir dosyayý önceki commit'ten geri getir
git checkout 79a5a4d -- KaynakMakinesi.UI/Program.cs

# Deðiþikliði gözden geçir
git diff --staged

# Ýptal etmek isterseniz:
git checkout HEAD -- KaynakMakinesi.UI/Program.cs
```

**Kullaným Senaryosu:**
- "Bu dosyanýn eski halini geri istiyorum"
- Yanlýþlýkla silinen kod parçasýný kurtarma
- Baþarýsýz bir deðiþikliði geri alma

---

## ?? Geri Yükleme Stratejileri

### Senaryo 1: "Son commit'i geri almak istiyorum"

#### A. Soft Reset (Deðiþiklikleri Koru)
```bash
# Commit'i geri al ama deðiþiklikleri working directory'de tut
git reset --soft HEAD~1

# Deðiþiklikler staged durumda kalýr
# Tekrar commit edebilir veya düzenleyebilirsiniz
```

#### B. Mixed Reset (Default - Deðiþiklikleri Unstage Et)
```bash
# Commit'i geri al, deðiþiklikleri unstaged yap
git reset HEAD~1
# veya
git reset --mixed HEAD~1

# Deðiþiklikler working directory'de var ama staged deðil
```

#### C. Hard Reset (HER ÞEYÝ SÝL - DÝKKAT!)
```bash
# ?? TEHLÝKELÝ: Commit'i VE deðiþiklikleri tamamen sil
git reset --hard HEAD~1

# Kullanmadan önce iki kere düþünün!
# Geri dönüþü yoktur (reflog hariç)
```

**Önerilen Kullaným:**
- ? Local branch'te çalýþýyorsanýz: Soft/Mixed kullanýn
- ? Push edilmiþ commit'leri ASLA hard reset yapmayýn!

---

### Senaryo 2: "Push edilmiþ bir commit'i geri almak istiyorum"

```bash
# Revert kullanýn (güvenli yöntem)
git revert <commit-hash>

# Örnek:
git revert 47051c5

# Bu yeni bir "geri alma commit'i" oluþturur
# Geçmiþ korunur, güvenlidir
```

**Neden Revert?**
- Ekip çalýþmasýnda güvenli
- Geçmiþi bozmaz
- GitHub/remote'ta sorun çýkarmaz

---

### Senaryo 3: "Birden fazla commit'i geri almak istiyorum"

```bash
# Son 3 commit'i geri al
git reset --soft HEAD~3

# veya belirli bir commit'e kadar geri git
git reset --soft 79a5a4d

# Tüm deðiþiklikleri gözden geçir
git status

# Yeniden commit et
git commit -m "refactor: önceki 3 commit birleþtirildi"
```

---

## ?? Branch Stratejisi (Önerilen)

### Geliþtirme Ýçin Branch Yapýsý

```bash
# Ana branch'ler
master (main)           # Production-ready kod
??? develop             # Geliþtirme branch'i
?   ??? feature/tag-monitor    # Yeni özellik
?   ??? feature/plc-optimizer  # Yeni özellik
?   ??? bugfix/null-ref-fix    # Bug düzeltme
?   ??? hotfix/critical-error  # Acil düzeltme
```

### Uygulama

```bash
# 1. Develop branch oluþtur (henüz yoksa)
git checkout -b develop

# 2. Yeni özellik için branch
git checkout -b feature/tag-monitor develop

# 3. Geliþtirme yap, commit et
git add .
git commit -m "feat: tag monitoring sistemi eklendi"

# 4. Develop'a merge et
git checkout develop
git merge feature/tag-monitor

# 5. Test ettikten sonra master'a merge
git checkout master
git merge develop

# 6. Push et
git push origin master
git push origin develop
```

---

## ?? Acil Durum Senaryolarý

### Durum 1: "Kodumu kaybettim, commit yapmamýþtým!"

```bash
# Git stash kontrol et
git stash list

# Varsa geri yükle
git stash pop

# Reflog'a bak (Git her þeyi tutar)
git reflog

# Kaybolan commit'i bul ve geri yükle
git checkout <commit-hash>
```

---

### Durum 2: "Yanlýþ branch'te commit yaptým!"

```bash
# Commit'i baþka branch'e taþý

# 1. Yanlýþ yaptýðýnýz commit'in hash'ini kopyalayýn
git log --oneline -1

# 2. Doðru branch'e geçin
git checkout correct-branch

# 3. Commit'i bu branch'e getirin
git cherry-pick <commit-hash>

# 4. Eski branch'ten commit'i silin
git checkout wrong-branch
git reset --hard HEAD~1
```

---

### Durum 3: "Merge conflict çýktý, ne yapmalýyým?"

```bash
# 1. Conflict olan dosyalarý listele
git status

# 2. Her dosyayý düzenle (<<<<<<, ======, >>>>>> iþaretlerini temizle)
# Visual Studio'da Merge Tool kullanabilirsiniz

# 3. Conflict çözüldükten sonra
git add .
git commit -m "merge: conflict çözüldü"

# Merge'i iptal etmek isterseniz:
git merge --abort
```

---

## ?? Best Practices (En Ýyi Uygulamalar)

### 1. Commit Mesajý Formatý

```bash
# Ýyi örnekler:
git commit -m "feat: yeni tag monitoring özelliði eklendi"
git commit -m "fix: NullReferenceException hatasý düzeltildi"
git commit -m "refactor: ModbusCodec sýnýfý yeniden yapýlandýrýldý"
git commit -m "docs: README.md güncellendi"
git commit -m "test: TagService unit testleri eklendi"

# Kötü örnekler (kullanmayýn):
git commit -m "deðiþiklikler"
git commit -m "bug fix"
git commit -m "update"
```

### Commit Prefix'leri:
- `feat:` - Yeni özellik
- `fix:` - Bug düzeltme
- `refactor:` - Kod yeniden yapýlandýrma
- `docs:` - Dokümantasyon
- `test:` - Test ekleme/güncelleme
- `chore:` - Bakým iþleri
- `style:` - Kod formatý
- `perf:` - Performans iyileþtirmesi

---

### 2. Sýk Commit Yapýn

```bash
# ? Ýyi
git add KaynakMakinesi.Core/Settings/AppSettings.cs
git commit -m "feat: AppSettings validation eklendi"

git add KaynakMakinesi.Infrastructure/Settings/JsonFileSettingsStore.cs
git commit -m "feat: JsonFileSettingsStore validation entegrasyonu"

# ? Kötü (tüm deðiþiklikleri tek commit'te)
git add .
git commit -m "birçok deðiþiklik"
```

**Neden?**
- Geri almak kolay
- Code review daha net
- Hata bulma kolaylaþýr

---

### 3. Push Etmeden Önce Test Edin

```bash
# Workflow:
# 1. Deðiþiklik yap
# 2. Build et
dotnet build  # veya Visual Studio'da Build

# 3. Lokal test
# 4. Commit
git commit -m "feat: yeni özellik"

# 5. Push
git push origin master
```

---

### 4. .gitignore Kullanýn

Zaten `.gitignore` dosyanýz var. Þunlar otomatik ignore ediliyor:
- `bin/`, `obj/` klasörleri
- `*.user`, `*.suo` dosyalarý
- `*.db` (SQLite veritabanlarý)
- `appsettings.json` (kullanýcý ayarlarý)

---

### 5. Branch'leri Düzenli Temizleyin

```bash
# Birleþtirilmiþ branch'leri listele
git branch --merged

# Eski branch'i sil
git branch -d feature/old-feature

# Remote branch'i sil
git push origin --delete feature/old-feature
```

---

## ?? Pratik Örnekler

### Örnek 1: "Önceki commit'teki NullLogger kodunu görmek istiyorum"

```bash
# 1. Commit'leri listele
git log --oneline --all | grep -i "null"

# 2. Ýlgili commit'i bul (örnek: 47051c5)
git show 47051c5:KaynakMakinesi.Infrastructure/Logging/NullLogger.cs

# Terminal'de kod görünür
```

---

### Örnek 2: "Tag özelliðini geliþtirmek için yeni branch oluþturuyorum"

```bash
# 1. Master'dan güncel branch oluþtur
git checkout master
git pull origin master
git checkout -b feature/tag-advanced-monitoring

# 2. Geliþtir
# ... kod yaz ...

# 3. Commit et
git add .
git commit -m "feat: geliþmiþ tag monitoring özellikleri"

# 4. Push et
git push origin feature/tag-advanced-monitoring

# 5. GitHub'da Pull Request oluþtur
```

---

### Örnek 3: "Production'da kritik bug var, acil hotfix!"

```bash
# 1. Master'dan hotfix branch oluþtur
git checkout master
git checkout -b hotfix/critical-plc-disconnect

# 2. Hýzlýca düzelt
# ... kod düzelt ...

# 3. Test et
# ... test ...

# 4. Commit
git commit -m "fix: kritik PLC disconnect hatasý düzeltildi"

# 5. Master'a merge
git checkout master
git merge hotfix/critical-plc-disconnect

# 6. Tag oluþtur (versiyon)
git tag -a v1.1.1 -m "Hotfix: PLC disconnect"

# 7. Push (tag ile birlikte)
git push origin master --tags

# 8. Develop'a da merge et
git checkout develop
git merge hotfix/critical-plc-disconnect
git push origin develop

# 9. Hotfix branch'i temizle
git branch -d hotfix/critical-plc-disconnect
```

---

## ?? Git Reflog - Zaman Makinesi

Git reflog, Git'in "geri alma" düðmesidir. Her þeyi tutar!

```bash
# Tüm iþlem geçmiþini göster
git reflog

# Örnek çýktý:
# 47051c5 (HEAD -> master) HEAD@{0}: commit: feat: kritik iyileþtirmeler
# 79a5a4d HEAD@{1}: commit: Modbus düzenlemesi
# bbc0133 HEAD@{2}: commit: Tag iþlemleri

# Belirli bir noktaya dön
git reset --hard HEAD@{2}

# veya
git reset --hard 79a5a4d
```

**Kullaným Senaryolarý:**
- Yanlýþlýkla hard reset yaptýnýz
- Branch sildiniz ama geri istiyorsunuz
- "1 saat önce ne yaptým?" sorusu

---

## ?? Git Alias'lar (Kýsayollar)

Git komutlarýný kýsaltmak için:

```bash
# Alias'larý ayarla (tek seferlik)
git config --global alias.st status
git config --global alias.co checkout
git config --global alias.br branch
git config --global alias.ci commit
git config --global alias.unstage 'reset HEAD --'
git config --global alias.last 'log -1 HEAD'
git config --global alias.visual 'log --oneline --graph --all --decorate'

# Kullaným:
git st           # git status yerine
git co master    # git checkout master yerine
git visual       # güzel log görünümü
```

---

## ??? Güvenlik Kontrol Listesi

Commit yapmadan önce:

- [ ] Build baþarýlý mý?
- [ ] Hassas bilgi yok mu? (þifreler, API key'ler)
- [ ] `.gitignore` doðru çalýþýyor mu?
- [ ] Commit mesajý açýklayýcý mý?
- [ ] Test edildi mi?

Push yapmadan önce:

- [ ] Remote branch güncel mi? (`git pull`)
- [ ] Conflict var mý?
- [ ] Kod review yapýldý mý?

---

## ?? Hýzlý Komut Referansý

```bash
# DURUM KONTROL
git status                          # Deðiþiklikleri göster
git log --oneline -10              # Son 10 commit
git diff                           # Deðiþiklikleri göster

# COMMIT & PUSH
git add .                          # Tüm deðiþiklikleri stage
git commit -m "mesaj"              # Commit
git push origin master             # Push

# BRANCH YÖNETÝMÝ
git checkout -b yeni-branch        # Yeni branch oluþtur
git checkout master                # Master'a geç
git merge feature-branch           # Branch'i merge et
git branch -d eski-branch          # Branch sil

# GERÝ ALMA
git reset --soft HEAD~1            # Son commit'i geri al (kodu koru)
git checkout -- dosya.cs           # Dosyayý geri yükle
git revert <commit-hash>           # Commit'i geri al (güvenli)

# TEMÝZLÝK
git clean -fd                      # Untracked dosyalarý sil
git stash                          # Deðiþiklikleri geçici sakla
git stash pop                      # Saklananý geri getir

# REMOTE
git remote -v                      # Remote'larý listele
git fetch origin                   # Remote deðiþiklikleri al
git pull origin master             # Pull & merge
```

---

## ?? Daha Fazla Bilgi

- [Git Resmi Dokümantasyon](https://git-scm.com/doc)
- [GitHub Guides](https://guides.github.com/)
- [Git Branching Model](https://nvie.com/posts/a-successful-git-branching-model/)
- [Semantic Commit Messages](https://gist.github.com/joshbuchea/6f47e86d2510bce28f8e7f42ae84c716)

---

**?? Sonuç:**

Git öðrenmek zaman alýr ama çok deðerlidir. Baþlangýçta yukarýdaki temel komutlarý kullanýn, zamanla daha geliþmiþ özelliklere geçin.

**Unutmayýn:** Git'te hemen hemen her þey geri alýnabilir. Denemekten korkmayýn! ??
