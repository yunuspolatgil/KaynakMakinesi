# ?? Git Quick Reference - KaynakMakinesi

## ? Acil Durum Komutlarý

### "Kodumu Kaybettim!"
```bash
git reflog                    # Tüm iþlem geçmiþi
git checkout <commit-hash>    # O noktaya git
```

### "Yanlýþ Commit Yaptým!"
```bash
git reset --soft HEAD~1       # Commit'i geri al (kodu koru)
git reset --hard HEAD~1       # ?? Her þeyi sil
```

### "Merge Conflict Var!"
```bash
git merge --abort             # Merge'i iptal et
git checkout --theirs file    # Onlarýn versiyonunu al
git checkout --ours file      # Bizim versiyonu al
```

---

## ?? Günlük Kullaným

### Yeni Özellik Geliþtirme
```bash
git checkout -b feature/yeni-ozellik
# ... kod yaz ...
git add .
git commit -m "feat: yeni özellik eklendi"
git push origin feature/yeni-ozellik
```

### Bug Düzeltme
```bash
git checkout -b bugfix/hata-adi
# ... düzelt ...
git commit -m "fix: hata düzeltildi"
git checkout master
git merge bugfix/hata-adi
git push
```

---

## ?? Commit Arasý Gezinme

### Geçmiþe Bakma
```bash
git log --oneline -10                    # Son 10 commit
git show 47051c5                         # Commit detayý
git diff 79a5a4d..47051c5               # Ýki commit arasý fark
```

### Geçici Ýnceleme
```bash
git checkout 79a5a4d                     # Eski commit'e git
# ... incele ...
git checkout master                      # Geri dön
```

### Dosya Bazlý Geri Yükleme
```bash
git checkout 79a5a4d -- Program.cs       # Tek dosya geri yükle
git checkout HEAD -- Program.cs          # Ýptal et
```

---

## ?? Branch Yönetimi

```bash
# Oluþtur ve geç
git checkout -b yeni-branch

# Listele
git branch                               # Local
git branch -a                            # Hepsi (local + remote)

# Sil
git branch -d branch-adi                 # Local
git push origin --delete branch-adi     # Remote

# Merge
git checkout master
git merge feature-branch
```

---

## ?? Stash (Geçici Saklama)

```bash
git stash                                # Sakla
git stash list                           # Liste
git stash pop                            # Geri getir ve sil
git stash apply                          # Geri getir (stash'te kal)
git stash drop                           # Stash'i sil
```

---

## ?? Remote Ýþlemleri

```bash
git remote -v                            # Remote'larý göster
git fetch origin                         # Deðiþiklikleri al (merge etme)
git pull origin master                   # Al ve merge et
git push origin master                   # Gönder
git push --tags                          # Tag'leri de gönder
```

---

## ?? Faydalý Log Komutlarý

```bash
# Grafik görünüm
git log --oneline --graph --all --decorate

# Dosya geçmiþi
git log --follow -- Program.cs

# Deðiþiklikleri göster
git log -p -2

# Ýstatistikler
git log --stat

# Belirli tarih aralýðý
git log --since="2 weeks ago"
```

---

## ?? Projeye Özel Örnekler

### Scenario 1: Validation Öncesi Koda Dönmek
```bash
# Kritik iyileþtirmeler öncesi commit
git checkout 79a5a4d

# Kodu incele, test et
# Geri dön
git checkout master
```

### Scenario 2: Settings Dosyasýný Eski Haline Getir
```bash
git checkout 79a5a4d -- KaynakMakinesi.Core/Settings/AppSettings.cs
```

### Scenario 3: Hotfix Branch Oluþtur
```bash
git checkout master
git checkout -b hotfix/critical-bug
# ... düzelt ...
git commit -m "fix: critical bug"
git checkout master
git merge hotfix/critical-bug
git tag -a v1.1.1 -m "Hotfix"
git push --tags
```

---

## ??? Güvenli Geri Alma

### Push Edilmemiþ Commit'ler
```bash
git reset --soft HEAD~1       # Deðiþiklikleri koru
git reset HEAD~1              # Unstage yap
git reset --hard HEAD~1       # ?? SÝL (dikkatli!)
```

### Push Edilmiþ Commit'ler
```bash
git revert <commit-hash>      # Yeni "geri alma" commit'i
git push                      # Güvenli yöntem
```

---

## ?? Önemli Commit Hash'leri (KaynakMakinesi)

| Hash | Açýklama |
|------|----------|
| `47051c5` | Kritik iyileþtirmeler, validation, NullLogger |
| `79a5a4d` | Modbus düzenlemesi yedek |
| `bbc0133` | Tag iþlemleri tamam |
| `2931992` | Tag yönetim devam |
| `cfa7b9e` | Modbus adresleme OK |

---

## ?? Sakýn Yapma!

? **git push --force** (baþkalarýnýn çalýþtýðý branch'te)
? **git reset --hard** (push edilmiþ commit'lerde)
? **Hassas bilgileri commit etme** (þifreler, API key'ler)
? **bin/, obj/ klasörlerini commit etme**
? **Çok büyük dosyalarý commit etme** (>50MB)

---

## ? Mutlaka Yap!

? Sýk commit yap
? Açýklayýcý commit mesajlarý yaz
? Push'tan önce test et
? Branch kullan (master'da direkt çalýþma)
? .gitignore kullan

---

## ?? Git Alias Önerileri

Bunlarý bir kere ayarla, hayatýn kolaylaþsýn:

```bash
git config --global alias.st status
git config --global alias.co checkout
git config --global alias.br branch
git config --global alias.ci commit
git config --global alias.unstage 'reset HEAD --'
git config --global alias.last 'log -1 HEAD'
git config --global alias.visual 'log --oneline --graph --all --decorate'
git config --global alias.undo 'reset --soft HEAD~1'
```

Kullaným:
```bash
git st              # git status
git visual          # güzel log
git undo            # son commit'i geri al
```

---

## ?? Yardým Al

```bash
git help <command>          # Detaylý yardým
git <command> --help        # Ayný þey
git <command> -h            # Kýsa özet
```

---

**?? Ýpucu:** Bu dosyayý her zaman açýk tut, sýk kullanacaksýn!

**Son Güncelleme:** 2025-01-XX
