use clap::{Arg, Command, ArgMatches, ValueEnum};
use std::fs;
use std::path::Path;
use std::process::{Command as ProcessCommand, Stdio};
use std::thread;
use std::time::Duration;
use std::io::{self};
use zip::read::ZipArchive;
use chrono::Local;

#[derive(Clone, Debug, ValueEnum)]
enum StartMode {
    /// 直接提权启动进程
    Elevated,
    /// 执行指定名称的任务计划程序
    Task,
}

#[derive(Clone, Debug, ValueEnum)]
enum TaskAction {
    /// 检查任务是否存在
    Check,
    /// 创建新任务
    Create,
    /// 删除任务
    Delete,
    /// 列出所有任务
    List,
}

fn main() {
    let matches = Command::new("z_stranslate_host")
        .version("1.0.0")
        .author("ZGGSONG <zggsong@foxmail.com>")
        .about("程序更新和后台启动工具")
        .subcommand(
            Command::new("update")
                .about("更新程序")
                .arg(
                    Arg::new("archive")
                        .short('a')
                        .long("archive")
                        .value_name("PATH")
                        .help("缓存的压缩包路径")
                        .required(true)
                )
                .arg(
                    Arg::new("wait-time")
                        .short('w')
                        .long("wait-time")
                        .value_name("SECONDS")
                        .help("关闭进程等待时间（秒）")
                        .default_value("0")
                        .value_parser(clap::value_parser!(u64))
                )
                .arg(
                    Arg::new("clean")
                        .short('c')
                        .long("clean")
                        .action(clap::ArgAction::SetTrue)
                        .help("是否清理必要目录（保留 log、portable_config、tmp 目录）")
                )
                .arg(
                    Arg::new("process-name")
                        .short('p')
                        .long("process")
                        .value_name("NAME")
                        .help("要关闭的进程名称")
                )
                .arg(
                    Arg::new("auto-start")
                        .short('s')
                        .long("auto-start")
                        .action(clap::ArgAction::SetTrue)
                        .help("更新完成后自动启动程序")
                )
                .arg(
                    Arg::new("verbose")
                        .short('v')
                        .long("verbose")
                        .action(clap::ArgAction::SetTrue)
                        .help("显示详细输出")
                )
        )
        .subcommand(
            Command::new("start")
                .about("后台启动程序")
                .arg(
                    Arg::new("mode")
                        .short('m')
                        .long("mode")
                        .value_name("MODE")
                        .help("启动方式")
                        .value_parser(clap::value_parser!(StartMode))
                        .default_value("elevated")
                )
                .arg(
                    Arg::new("target")
                        .short('t')
                        .long("target")
                        .value_name("PATH_OR_TASK")
                        .help("目标程序路径或任务计划名称")
                        .required(true)
                )
                .arg(
                    Arg::new("args")
                        .short('a')
                        .long("args")
                        .value_name("ARGUMENTS")
                        .help("启动参数")
                        .action(clap::ArgAction::Append)
                )
                .arg(
                    Arg::new("delay")
                        .short('d')
                        .long("delay")
                        .value_name("SECONDS")
                        .help("启动延迟（秒）")
                        .default_value("0")
                        .value_parser(clap::value_parser!(u64))
                )
                .arg(
                    Arg::new("verbose")
                        .short('v')
                        .long("verbose")
                        .action(clap::ArgAction::SetTrue)
                        .help("显示详细输出")
                )
        )
        .subcommand(
            Command::new("task")
                .about("管理Windows任务计划")
                .arg(
                    Arg::new("action")
                        .short('a')
                        .long("action")
                        .value_name("ACTION")
                        .help("操作类型")
                        .value_parser(clap::value_parser!(TaskAction))
                        .required(true)
                )
                .arg(
                    Arg::new("name")
                        .short('n')
                        .long("name")
                        .value_name("TASK_NAME")
                        .help("任务计划名称")
                        .required_if_eq("action", "create")
                        .required_if_eq("action", "check")
                        .required_if_eq("action", "delete")
                )
                .arg(
                    Arg::new("program")
                        .short('p')
                        .long("program")
                        .value_name("PATH")
                        .help("要执行的程序路径（创建任务时需要）")
                )
                .arg(
                    Arg::new("working-dir")
                        .short('w')
                        .long("working-dir")
                        .value_name("PATH")
                        .help("工作目录（可选，默认为程序所在目录）")
                )
                .arg(
                    Arg::new("description")
                        .short('d')
                        .long("description")
                        .value_name("TEXT")
                        .help("任务描述")
                        .default_value("just for jump uac")
                )
                .arg(
                    Arg::new("run-level")
                        .short('r')
                        .long("run-level")
                        .value_name("LEVEL")
                        .help("运行级别")
                        .value_parser(["limited", "highest"])
                        .default_value("highest")
                )
                .arg(
                    Arg::new("force")
                        .short('f')
                        .long("force")
                        .action(clap::ArgAction::SetTrue)
                        .help("强制操作（覆盖已存在的任务或强制删除）")
                )
                .arg(
                    Arg::new("verbose")
                        .short('v')
                        .long("verbose")
                        .action(clap::ArgAction::SetTrue)
                        .help("显示详细输出")
                )
        )
        .get_matches();

    match matches.subcommand() {
        Some(("update", sub_matches)) => {
            if let Err(e) = handle_update_command(sub_matches) {
                eprintln!("❌ 更新失败: {}", e);
                std::process::exit(1);
            }
        }
        Some(("start", sub_matches)) => {
            if let Err(e) = handle_start_command(sub_matches) {
                eprintln!("❌ 启动失败: {}", e);
                std::process::exit(1);
            }
        }
        Some(("task", sub_matches)) => {
            if let Err(e) = handle_task_command(sub_matches) {
                eprintln!("❌ 任务操作失败: {}", e);
                std::process::exit(1);
            }
        }
        _ => {
            eprintln!("❌ 请指定命令: update、start 或 task");
            eprintln!("使用 --help 查看帮助信息");
            std::process::exit(1);
        }
    }
}

fn handle_task_command(matches: &ArgMatches) -> Result<(), Box<dyn std::error::Error>> {
    let action = matches.get_one::<TaskAction>("action").unwrap();
    let verbose = matches.get_flag("verbose");

    #[cfg(target_os = "windows")]
    {
        match action {
            TaskAction::Check => {
                let task_name = matches.get_one::<String>("name").unwrap();
                check_task_exists(task_name, verbose)?;
            }
            TaskAction::Create => {
                let task_name = matches.get_one::<String>("name").unwrap();
                let program = matches.get_one::<String>("program")
                    .ok_or("创建任务时必须指定程序路径 --program")?;
                let working_dir = matches.get_one::<String>("working-dir");
                let description = matches.get_one::<String>("description").unwrap();
                let run_level = matches.get_one::<String>("run-level").unwrap();
                let force = matches.get_flag("force");
                
                create_task(task_name, program, working_dir, description, run_level, force, verbose)?;
            }
            TaskAction::Delete => {
                let task_name = matches.get_one::<String>("name").unwrap();
                delete_task(task_name, verbose)?;
            }
            TaskAction::List => {
                list_tasks(verbose)?;
            }
        }
    }

    Ok(())
}

#[cfg(target_os = "windows")]
fn check_task_exists(task_name: &str, verbose: bool) -> Result<(), Box<dyn std::error::Error>> {
    if verbose {
        println!("🔍 检查任务计划是否存在: {}", task_name);
    }

    let output = ProcessCommand::new("schtasks")
        .args(&["/Query", "/TN", task_name])
        .output()?;

    if output.status.success() {
        println!("✅ 任务计划存在: {}", task_name);
        if verbose {
            let info = String::from_utf8_lossy(&output.stdout);
            println!("📋 任务信息:");
            println!("{}", info);
        }
    } else {
        println!("❌ 任务计划不存在: {}", task_name);
    }

    Ok(())
}

#[cfg(target_os = "windows")]
fn create_task(
    task_name: &str,
    program: &str,
    working_dir: Option<&String>,
    description: &str,
    run_level: &str,
    force: bool,
    verbose: bool,
) -> Result<(), Box<dyn std::error::Error>> {
    if verbose {
        println!("📝 创建任务计划: {}", task_name);
        println!("   程序路径: {}", program);
        if let Some(wd) = working_dir {
            println!("   工作目录: {}", wd);
        }
        println!("   运行级别: {}", run_level);
    }

    // 检查程序是否存在
    if !Path::new(program).exists() {
        return Err(format!("程序文件不存在: {}", program).into());
    }

    // 确定工作目录
    let work_dir = if let Some(wd) = working_dir {
        wd.clone()
    } else {
        Path::new(program)
            .parent()
            .ok_or("无法确定程序所在目录")?
            .to_string_lossy()
            .to_string()
    };

    // 检查任务是否已存在
    let check_output = ProcessCommand::new("schtasks")
        .args(&["/Query", "/TN", task_name])
        .output()?;

    if check_output.status.success() && !force {
        println!("✅ 任务计划已存在: {}，使用 --force 强制覆盖", task_name);
        return Ok(());
    }

    // 生成XML内容
    let xml_content = generate_task_xml(task_name, program, &work_dir, description, run_level)?;
    
    // 创建临时XML文件
    let temp_xml_path = format!("temp_task_{}.xml", task_name);
    fs::write(&temp_xml_path, xml_content)?;

    if verbose {
        println!("📄 已生成临时XML文件: {}", temp_xml_path);
    }

    // 创建任务计划
    let create_args = vec!["/Create", "/XML", &temp_xml_path, "/TN", task_name, "/F"];

    let output = ProcessCommand::new("schtasks")
        .args(&create_args)
        .output()?;

    // 清理临时文件
    let _ = fs::remove_file(&temp_xml_path);
    if verbose {
        println!("🗑️ 已删除临时XML文件: {}", temp_xml_path);
    }

    if output.status.success() {
        println!("✅ 任务计划创建成功: {}", task_name);
        if verbose {
            let result = String::from_utf8_lossy(&output.stdout);
            println!("📋 创建结果: {}", result);
        }
    } else {
        let error = String::from_utf8_lossy(&output.stderr);
        return Err(format!("创建任务计划失败: {}", error).into());
    }

    Ok(())
}

#[cfg(target_os = "windows")]
fn delete_task(task_name: &str, verbose: bool) -> Result<(), Box<dyn std::error::Error>> {
    if verbose {
        println!("🗑️  删除任务计划: {}", task_name);
    }

    // 检查任务计划是否存在
    let check_output = ProcessCommand::new("schtasks")
        .args(&["/Query", "/TN", task_name])
        .output()?;
    
    if !check_output.status.success() {
        println!("✅ 任务计划不存在: {}", task_name);
        return Ok(());
    }

    let args = vec!["/Delete", "/TN", task_name, "/F"];
    
    let output = ProcessCommand::new("schtasks")
        .args(&args)
        .output()?;

    if output.status.success() {
        println!("✅ 任务计划删除成功: {}", task_name);
    } else {
        let error = String::from_utf8_lossy(&output.stderr);
        return Err(format!("删除任务计划失败: {}", error).into());
    }

    Ok(())
}

#[cfg(target_os = "windows")]
fn list_tasks(verbose: bool) -> Result<(), Box<dyn std::error::Error>> {
    if verbose {
        println!("📋 列出所有任务计划...");
    }

    let output = ProcessCommand::new("schtasks")
        .args(&["/Query", "/FO", "TABLE"])
        .output()?;

    if output.status.success() {
        let tasks = String::from_utf8_lossy(&output.stdout);
        println!("📋 任务计划列表:");
        println!("{}", tasks);
    } else {
        let error = String::from_utf8_lossy(&output.stderr);
        return Err(format!("获取任务列表失败: {}", error).into());
    }

    Ok(())
}

#[cfg(target_os = "windows")]
fn generate_task_xml(
    task_name: &str,
    program: &str,
    working_dir: &str,
    description: &str,
    run_level: &str,
) -> Result<String, Box<dyn std::error::Error>> {
    let current_time = Local::now().format("%Y-%m-%dT%H:%M:%S").to_string();
    let run_level_value = if run_level == "highest" { "HighestAvailable" } else { "LeastPrivilege" };
    
    // 获取当前用户SID（简化处理，实际环境中可能需要更复杂的逻辑）
    let user_sid = get_current_user_sid().unwrap_or_else(|_| "S-1-5-32-544".to_string());

    let xml_content = format!(r#"<?xml version="1.0" encoding="UTF-16"?>
<Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
  <RegistrationInfo>
    <Date>{}</Date>
    <Author>stranslate - zggsong</Author>
    <Description>{}</Description>
    <URI>\{}</URI>
  </RegistrationInfo>
  <Triggers />
  <Principals>
    <Principal id="Author">
      <UserId>{}</UserId>
      <LogonType>InteractiveToken</LogonType>
      <RunLevel>{}</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
    <AllowHardTerminate>true</AllowHardTerminate>
    <StartWhenAvailable>false</StartWhenAvailable>
    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>
    <IdleSettings>
      <StopOnIdleEnd>true</StopOnIdleEnd>
      <RestartOnIdle>false</RestartOnIdle>
    </IdleSettings>
    <AllowStartOnDemand>true</AllowStartOnDemand>
    <Enabled>true</Enabled>
    <Hidden>false</Hidden>
    <RunOnlyIfIdle>false</RunOnlyIfIdle>
    <WakeToRun>false</WakeToRun>
    <ExecutionTimeLimit>PT72H</ExecutionTimeLimit>
    <Priority>4</Priority>
  </Settings>
  <Actions Context="Author">
    <Exec>
      <Command>{}</Command>
      <WorkingDirectory>{}</WorkingDirectory>
    </Exec>
  </Actions>
</Task>"#,
        current_time,
        description,
        task_name,
        user_sid,
        run_level_value,
        program,
        working_dir
    );

    Ok(xml_content)
}

#[cfg(target_os = "windows")]
fn get_current_user_sid() -> Result<String, Box<dyn std::error::Error>> {
    let output = ProcessCommand::new("whoami")
        .args(&["/user", "/fo", "csv", "/nh"])
        .output()?;

    if output.status.success() {
        let result = String::from_utf8_lossy(&output.stdout);
        // CSV格式："用户名","SID"
        if let Some(sid_part) = result.split(',').nth(1) {
            let sid = sid_part.trim().trim_matches('"');
            return Ok(sid.to_string());
        }
    }

    Err("无法获取当前用户SID".into())
}

fn handle_update_command(matches: &ArgMatches) -> Result<(), Box<dyn std::error::Error>> {
    let archive_path = matches.get_one::<String>("archive").unwrap();
    let wait_time = *matches.get_one::<u64>("wait-time").unwrap();
    let should_clean = matches.get_flag("clean");
    let process_name = matches.get_one::<String>("process-name");
    let auto_start = matches.get_flag("auto-start");
    let verbose = matches.get_flag("verbose");

    if verbose {
        println!("🔧 开始更新程序...");
        println!("   压缩包路径: {}", archive_path);
        if wait_time > 0 {
            println!("   等待时间: {} 秒", wait_time);
        }
        println!("   清理目录: {}", should_clean);
        println!("   自动启动: {}", auto_start);
    }

    // 1. 检查压缩包是否存在
    if !Path::new(archive_path).exists() {
        return Err(format!("压缩包不存在: {}", archive_path).into());
    }

    // 2. 关闭指定进程
    if let Some(process) = process_name {
        if verbose {
            println!("🔄 正在关闭进程: {}", process);
        }
        close_process(process, verbose)?;
    }

    // 3. 等待指定时间
    if wait_time > 0 {
        if verbose {
            println!("⏳ 等待 {} 秒...", wait_time);
        }
        thread::sleep(Duration::from_secs(wait_time));
    }

    // 4. 使用你的解压逻辑
    unzip_file_to_parent_dir(archive_path, should_clean)?;

    if verbose {
        println!("✅ 解压完成");
    }

    // 5. 自动启动程序（如果启用）
    if auto_start {
        // 截取tmp前的目录名
        let parent = Path::new(archive_path).parent()
            .and_then(|p| p.parent())
            .ok_or("无法确定程序目录")?;
        
        // 拼接 STranslate.exe
        let exe_path = parent.join("STranslate.exe");

        if exe_path.exists() {
            if verbose {
                println!("🚀 启动 STranslate.exe...");
            }
            std::process::Command::new(&exe_path).spawn()?;
            println!("✅ 程序已启动");
        } else if verbose {
            println!("⚠️  STranslate.exe 不存在，跳过自动启动");
        }
    }

    println!("✅ 更新完成!");
    Ok(())
}

fn handle_start_command(matches: &ArgMatches) -> Result<(), Box<dyn std::error::Error>> {
    let mode = matches.get_one::<StartMode>("mode").unwrap();
    let target = matches.get_one::<String>("target").unwrap();
    let args: Vec<&String> = matches.get_many::<String>("args").unwrap_or_default().collect();
    let delay = *matches.get_one::<u64>("delay").unwrap();
    let verbose = matches.get_flag("verbose");

    if verbose {
        println!("🚀 准备启动程序...");
        println!("   启动方式: {:?}", mode);
        println!("   目标: {}", target);
        if !args.is_empty() {
            println!("   参数: {:?}", args);
        }
        if delay > 0 {
            println!("   延迟: {} 秒", delay);
        }
    }

    // 启动延迟
    if delay > 0 {
        if verbose {
            println!("⏳ 延迟 {} 秒后启动...", delay);
        }
        thread::sleep(Duration::from_secs(delay));
    }

    match mode {
        StartMode::Elevated => {
            start_elevated_process(target, &args, verbose)?;
        }
        StartMode::Task => {
            start_task_scheduler(target, verbose)?;
        }
    }

    println!("✅ 启动完成!");
    Ok(())
}

/// 你的原始解压函数，完全保留你的逻辑
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
    
    Ok(())
}

fn close_process(process_name: &str, verbose: bool) -> Result<(), Box<dyn std::error::Error>> {
    if verbose {
        println!("🔄 正在关闭进程: {}", process_name);
    }

    // Windows: 使用 taskkill
    #[cfg(target_os = "windows")]
    {
        let output = ProcessCommand::new("taskkill")
            .args(&["/F", "/IM", process_name])
            .output()?;
        
        if !output.status.success() {
            let error = String::from_utf8_lossy(&output.stderr);
            if verbose {
                println!("⚠️  进程可能已经关闭或不存在: {}", error);
            }
        } else if verbose {
            println!("✅ 进程已关闭: {}", process_name);
        }
    }

    Ok(())
}

fn start_elevated_process(target: &str, args: &[&String], verbose: bool) -> Result<(), Box<dyn std::error::Error>> {
    if verbose {
        println!("🔑 以提权方式启动进程: {}", target);
    }

    // Windows: 使用 Start-Process 以管理员权限运行
    #[cfg(target_os = "windows")]
    {
        let mut cmd_args = vec![
            "-Command".to_string(),
            format!("Start-Process '{}' -Verb RunAs", target)
        ];
        
        if !args.is_empty() {
            let args_str = args.iter().map(|s| s.as_str()).collect::<Vec<_>>().join(" ");
            cmd_args[1] = format!("Start-Process '{}' -ArgumentList '{}' -Verb RunAs", target, args_str);
        }

        let mut command = ProcessCommand::new("powershell");
        command.args(&cmd_args);
        
        if !verbose {
            command.stdout(Stdio::null()).stderr(Stdio::null());
        }
        
        command.spawn()?;
    }

    Ok(())
}

fn start_task_scheduler(task_name: &str, verbose: bool) -> Result<(), Box<dyn std::error::Error>> {
    if verbose {
        println!("📅 启动任务计划: {}", task_name);
    }

    // Windows: 使用 schtasks
    #[cfg(target_os = "windows")]
    {
        let output = ProcessCommand::new("schtasks")
            .args(&["/Run", "/TN", task_name])
            .output()?;
        
        if !output.status.success() {
            let error = String::from_utf8_lossy(&output.stderr);
            return Err(format!("启动任务计划失败: {}", error).into());
        }
        
        if verbose {
            println!("✅ 任务计划已启动: {}", task_name);
        }
    }

    Ok(())
}