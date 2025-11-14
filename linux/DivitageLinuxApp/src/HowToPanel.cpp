#include "HowToPanel.h"

#include <QLabel>
#include <QScrollArea>
#include <QVBoxLayout>

HowToPanel::HowToPanel(QWidget *parent)
    : QWidget(parent)
{
    auto *scroll = new QScrollArea(this);
    scroll->setWidgetResizable(true);

    auto *container = new QWidget(scroll);
    auto *layout = new QVBoxLayout(container);
    layout->setSpacing(12);

    layout->addWidget(new QLabel(tr("<h2>使い方</h2>")));

    const QStringList steps = {
        tr("① 変換したいファイル / フォルダを準備します。"),
        tr("② メイン画面へドラッグ＆ドロップするか、ボタンから選択します。"),
        tr("③ Ctrl+Enter で変換を開始します。"),
        tr("④ 完了通知後、保存先を開きます。"),
    };
    for (const QString &step : steps) {
        layout->addWidget(new QLabel(step));
    }

    layout->addWidget(new QLabel(tr("<h3>ショートカット</h3>")));
    layout->addWidget(new QLabel(tr("Ctrl+Enter : 変換開始")));
    layout->addWidget(new QLabel(tr("Ctrl+1〜3 : コンバーター / 設定 / 使い方")));
    layout->addStretch();

    scroll->setWidget(container);

    auto *root = new QVBoxLayout(this);
    root->addWidget(scroll);
}
