use std::time::Duration;
use std::{env, fs, thread};
use std::io::{self};
use std::path::Path;
use zip::read::ZipArchive;


fn unzip_file_to_parent_dir(zip_path: &str, clear_dir: bool) -> io::Result<()> {
    // 获取ZIP文件路径
    let zip_path = Path::new(zip_path);
    
    // 确保文件存在且是ZIP文件
    if !zip_path.exists() || zip_path.extension().unwrap_or_default() != "zip" {
        return Err(io::Error::new(
            io::ErrorKind::InvalidInput, 
            "提供的路径不存在或不是ZIP文件"
        ));
    }
    
    // 获取压缩包所在目录的上级目录（祖父目录）
    let grand_parent_dir = match zip_path.parent().and_then(|dir| dir.parent()) {
        Some(grand_parent) => grand_parent,
        None => return Err(io::Error::new(io::ErrorKind::NotFound, "无法确定上上级目录"))
    };

    // 如果需要清空目录
    if clear_dir {
        // 要保留的目录列表
        let skip_dirs = ["log", "portable_config", "tmp"];
        
        // 清空目录中的所有文件和文件夹，但跳过指定目录
        if let Ok(entries) = fs::read_dir(grand_parent_dir) {
            for entry in entries.flatten() {
                let path = entry.path();
                let name = path.file_name().and_then(|n| n.to_str()).unwrap_or("");
                
                // 如果是需要保留的目录，则跳过
                if skip_dirs.contains(&name) {
                    continue;
                }
                
                // 删除文件或目录
                if path.is_dir() {
                    fs::remove_dir_all(&path)?;
                } else {
                    fs::remove_file(&path)?;
                }
            }
        }
    }
    
    // 打开ZIP文件
    let file = fs::File::open(zip_path)?;
    let mut archive = ZipArchive::new(file)?;
    
    // 解压所有文件
    for i in 0..archive.len() {
        let mut file = archive.by_index(i)?;
        let outpath = grand_parent_dir.join(file.name());
        
        if file.name().ends_with('/') {
            // 是目录
            fs::create_dir_all(&outpath)?;
        } else {
            // 是文件
            if let Some(p) = outpath.parent() {
                if !p.exists() {
                    fs::create_dir_all(p)?;
                }
            }
            let mut outfile = fs::File::create(&outpath)?;
            io::copy(&mut file, &mut outfile)?;
        }
    }
    
    // println!("ZIP文件已成功解压到上上级目录: {:?}", grand_parent_dir);
    Ok(())
}

// 使用示例
fn main() -> io::Result<()> {
    // 从命令行参数获取ZIP文件路径
    let args: Vec<String> = env::args().collect();
    
    // 检查是否提供了参数
    let zip_path = if args.len() > 1 {
        args[1].clone()
    } else {
        // 如果没有提供参数，可以使用默认路径或者显示使用说明并退出
        return Err(io::Error::new(
            io::ErrorKind::InvalidInput, 
            "请提供ZIP文件路径作为第一个参数"
        ));
    };
  
    // 检查是否提供了等待时间参数
    let wait_seconds = if args.len() > 2 {
        match args[2].parse::<u64>() {
            Ok(seconds) => seconds,
            Err(_) => return Err(io::Error::new(
                io::ErrorKind::InvalidInput,
                "等待时间必须是有效的整数秒数"
            ))
        }
    } else {
        0 // 如果没有提供等待时间，默认为0秒
    };

    // 检查是否提供了清空目录参数
    let clear_dir = if args.len() > 3 {
        match args[3].parse::<bool>() {
            Ok(value) => value,
            Err(_) => return Err(io::Error::new(
                io::ErrorKind::InvalidInput,
                "清空目录参数必须是 true 或 false"
            ))
        }
    } else {
        false // 如果没有提供清空目录参数，默认为false
    };

    // 如果设置了等待时间，先等待
    if wait_seconds > 0 {
        println!("等待 {} 秒...", wait_seconds);
        thread::sleep(Duration::from_secs(wait_seconds));
    }
    
    // 解压示例
    unzip_file_to_parent_dir(&zip_path, clear_dir)?;
    
    // 截取tmp前的目录名
    let parent = Path::new(&zip_path).parent().unwrap().parent().unwrap();
    // 拼接 STranslate.exe
    let exe_path = parent.join("STranslate.exe");

    // 如果不存在，则返回错误
    if !exe_path.exists() {
        return Err(io::Error::new(
            io::ErrorKind::NotFound, 
            "STranslate.exe 不存在"
        ));
    }

    println!("启动 STranslate.exe...");
    std::process::Command::new(exe_path).spawn()?;

    Ok(())
}